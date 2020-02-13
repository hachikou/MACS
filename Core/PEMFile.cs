/// PEMFile: PEMファイル操作ユーティリティー
///
/// Copyright (C) 2019 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MACS {

    public class PEMFile : Loggable {

    public PEMFile() {
        Kind = PEMKind.Invalid;
    }

    public PEMFile(string filepath) {
        using(StreamReader sr = FileUtil.Reader(filepath, Encoding.ASCII)) {
            if(sr == null)
                throw new IOException(String.Format("Failed to open {0}", filepath));
            Load(sr);
        }
    }

    public PEMFile(Stream stream) {
        using(StreamReader sr = new StreamReader(stream, Encoding.ASCII)) {
            Load(sr);
        }
    }

    public PEMFile(StreamReader sr) {
        Load(sr);
    }

    public PEMKind Kind
    { get; private set; }

    public void Load(StreamReader sr) {
        Kind = PEMKind.Invalid;
        StringBuilder b64 = new StringBuilder();
        bool inData = false;
        while(!sr.EndOfStream) {
            string line = sr.ReadLine();
            if(line.StartsWith("-----BEGIN ")) {
                if(line.Contains("RSA PRIVATE KEY")) {
                    Kind = PEMKind.RSAPrivateKey;
                } else if(line.Contains("RSA PUBLIC KEY")) {
                    Kind = PEMKind.RSAPublicKey;
                } else if(line.Contains("PUBLIC KEY")) {
                    Kind = PEMKind.PublicKey;
                } else {
                    throw new InvalidOperationException("Invalid key type");
                }
                inData = true;
            } else if(line.StartsWith("-----END ")) {
                inData = false;
                break;
            } else if(inData) {
                b64.Append(line);
            }
        }
        if((Kind == PEMKind.Invalid) || (b64.Length == 0))
            throw new InvalidOperationException("Bad file format");
        der = Convert.FromBase64String(b64.ToString());
    }

    public RSAParameters GetRSAParameters() {
        try {
            switch(Kind) {
            case PEMKind.RSAPrivateKey:
                return getRSAPrivateKey();
            case PEMKind.RSAPublicKey:
                return getRSAPublicKey();
            case PEMKind.PublicKey:
                return getPublicKey();
            }
        } catch(Exception ex) {
            LOG_EXCEPTION(ex);
        }
        return new RSAParameters(); // Fail-safe
    }


    private byte[] der;

    private RSAParameters getRSAPrivateKey() {
        byte[] sequence;
        using(var reader = new BinaryReader(new MemoryStream(der))) {
            sequence = read(reader);
        }
        var parameters = new RSAParameters();
        using(var reader = new BinaryReader(new MemoryStream(sequence))) {
            read(reader); // version
            parameters.Modulus = read(reader);
            parameters.Exponent = read(reader);
            parameters.D = read(reader);
            parameters.P = read(reader);
            parameters.Q = read(reader);
            parameters.DP = read(reader);
            parameters.DQ = read(reader);
            parameters.InverseQ = read(reader);
        }
        return parameters;
    }

    private RSAParameters getRSAPublicKey() {
        byte[] sequence3;
        using(var reader = new BinaryReader(new MemoryStream(der))) {
            sequence3 = read(reader);
        }
        var parameters = new RSAParameters();
        using(var reader = new BinaryReader(new MemoryStream(sequence3))) {
            parameters.Modulus = read(reader);
            parameters.Exponent = read(reader);
        }
        return parameters;
    }

    private RSAParameters getPublicKey() {
        byte[] sequence1;
        using (var reader = new BinaryReader(new MemoryStream(der))) {
            sequence1 = read(reader);
        }
        byte[] sequence2;
        using (var reader = new BinaryReader(new MemoryStream(sequence1))) {
            read(reader); // sequence
            sequence2 = read(reader); // bit string
        }
        byte[] sequence3;
        using (var reader = new BinaryReader(new MemoryStream(sequence2))) {
            sequence3 = read(reader); // sequence
        }

        var parameters = new RSAParameters();
        using (var reader = new BinaryReader(new MemoryStream(sequence3))) {
            parameters.Modulus = read(reader);
            parameters.Exponent = read(reader);
        }

        return parameters;
    }

    private byte[] read(BinaryReader reader) {
        // tag
        reader.ReadByte();
        // length
        int length = 0;
        byte b = reader.ReadByte();
        if((b&0x80) == 0x80) { // length >= 128
            int n = b&0x7f;
            byte[] buf = new byte[] {0x00, 0x00, 0x00, 0x00};
            for(int i = n-1; i >= 0; i--)
                buf[i] = reader.ReadByte();
            length = BitConverter.ToInt32(buf, 0);
        } else { // length < 128
            length = b;
        }
        // value
        if(length == 0)
            return new byte[0];
        byte first = reader.ReadByte();
        if(first == 0x00)
            length -= 1; // 最上位byteが0x00の場合は除いておく
        else
            reader.BaseStream.Seek(-1, SeekOrigin.Current); // 1byte読んじゃったので戻しておく
        return reader.ReadBytes(length);
    }
    
}

public enum PEMKind {
    Invalid = 0,
    RSAPrivateKey,
    RSAPublicKey,
    PublicKey,
}

} // End of namespace
