DROP TABLE IF EXISTS サンプルテーブル;
CREATE TABLE サンプルテーブル (
       ID INTEGER,
       名前 VARCHAR(32),
       電話番号 VARCHAR(16) );
INSERT INTO サンプルテーブル
       (ID, 名前, 電話番号)
       VALUES(1, 'テスト一郎', '03-4567-8901');
INSERT INTO サンプルテーブル
       (ID, 名前, 電話番号)
       VALUES(2, 'テスト次郎', '03-4444-5555');
INSERT INTO サンプルテーブル
       (ID, 名前, 電話番号)
       VALUES(3, 'テスト三郎', '03-6666-7777');
INSERT INTO サンプルテーブル
       (ID, 名前, 電話番号)
       VALUES(100, '山田太郎', '');
INSERT INTO サンプルテーブル
       (ID, 名前, 電話番号)
       VALUES(101, '山田花子', '090-0000-1111');
INSERT INTO サンプルテーブル
       (ID, 名前, 電話番号)
       VALUES(102, '鈴木一郎', '090-9876-5432');

DROP TABLE IF EXISTS サンプル役職;
CREATE TABLE サンプル役職 (
       ID INTEGER,
       役職 VARCHAR(32) );
INSERT INTO サンプル役職
       (ID, 役職)
       VALUES(1, '技術課長');
INSERT INTO サンプル役職
       (ID, 役職)
       VALUES(2, '技術課長');
INSERT INTO サンプル役職
       (ID, 役職)
       VALUES(101, '技術本部長');
INSERT INTO サンプル役職
       (ID, 役職)
       VALUES(101, '営業本部長');
INSERT INTO サンプル役職
       (ID, 役職)
       VALUES(102, '取締役');

DROP TABLE IF EXISTS サンプル翻訳;
CREATE TABLE サンプル翻訳 (
       日本語 VARCHAR(64),
       英語 VARCHAR(64) );
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('テスト一郎', 'Test Ichiro');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('テスト次郎', 'Test Jiro');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('テスト三郎', 'Test Saburou');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('山田太郎', 'Yamada Taro');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('山田花子', 'Yamada Hanako');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('鈴木一郎', 'Suzuki Ichiro');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('技術課長', 'Technical Leader');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('技術本部長', 'Technical Manager');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('営業本部長', 'Sales Manager');
INSERT INTO サンプル翻訳
       (日本語, 英語)
       VALUES('取締役', 'President');

DROP TABLE IF EXISTS サンプル給与;
CREATE TABLE サンプル給与 (
       ID INTEGER,
       給与 DECIMAL(10,2) );
