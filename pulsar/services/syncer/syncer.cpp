#include "syncer.h"

#include <chrono>
#include <iostream>
#include <pulsar/services/config/appconfig.h>
#include <quark/business/articles/article.h>
#include <quark/business/articles/channel.h>
#include <quark/services/database/sqlite_service.h>
#include <quark/services/filesystem/filesystem.h>
#include <quark/services/logger/logger.h>
#include <thread>

void initDatabase(quark::MTSqliteService &sqliteService) {
  std::string initChannelsSqlText = R"sql(
        CREATE TABLE IF NOT EXISTS channels
        (
            urn TEXT PRIMARY KEY,
            name TEXT NOT NULL,
            description TEXT,
            image TEXT
        );
)sql";

  std::string initArticlesSqlText = R"sql(
        CREATE TABLE IF NOT EXISTS articles
        (
            urn TEXT PRIMARY KEY,
            title TEXT NOT NULL,
            header TEXT,
            body TEXT,
            create_time TEXT,
            update_time TEXT,
            creator TEXT,
            keywords TEXT,
            description TEXT,
            cover TEXT DEFAULT '',
            discover INTEGER DEFAULT 0,
            owner TEXT,
            channel TEXT,
            partition TEXT,
            path TEXT
        );
)sql";

  auto initSqlList = std::vector{initChannelsSqlText, initArticlesSqlText};
  sqliteService.runSqlBatch(initSqlList);
}

void syncChannels(quark::MTSqliteService &sqliteService) {

  auto sourcePath = pulsar::AppConfig::Default().GetSourcePath();
  const std::string baseUrl = quark::ResolvePath(sourcePath);

  auto channelServer = std::make_shared<quark::ChannelServerBusiness>(baseUrl);
  auto channelsPtr = channelServer->selectChannels();

  auto insertSqlText = R"sql(
    INSERT INTO channels (urn, name, description, image)
            VALUES ($urn, $name, $description, $image)
            ON CONFLICT(urn) DO UPDATE SET
                name = excluded.name,
                description = excluded.description,
                image = excluded.image
            WHERE channels.urn = excluded.urn;
)sql";

  auto sqlCommand = sqliteService.createCommand(insertSqlText);

  for (const auto &model : *channelsPtr) {
    // quark::Logger::LogInfo({model.URN, model.Name, model.Title});

    sqlCommand->BindString("$urn", model.URN);
    sqlCommand->BindString("$name", model.Name);
    sqlCommand->BindString("$description", model.Description);
    sqlCommand->BindString("$image", model.Image);
    sqlCommand->Run();
    sqlCommand->Reset();
  }
}

void syncArticles(quark::MTSqliteService &sqliteService) {
  auto sourcePath = pulsar::AppConfig::Default().GetSourcePath();
  const std::string baseUrl = quark::ResolvePath(sourcePath);

  auto channelServer = std::make_shared<quark::ChannelServerBusiness>(baseUrl);

  auto articleServer = std::make_shared<quark::ArticleFileService>(baseUrl);

  auto insertSqlText = R"sql(
INSERT INTO articles (urn, title, header, body, create_time, update_time, creator, keywords, description,
                cover, owner, channel, partition, path)
            VALUES ($urn, $title, $header, $body, $create_time, $update_time, $creator, $keywords, $description,
                $cover, $owner, $channel, $partition, $path)
            ON CONFLICT(urn) DO UPDATE SET
                title = excluded.title,
                header = excluded.header,
                body = excluded.body,
                create_time = excluded.create_time,
                update_time = excluded.update_time,
                creator = excluded.creator,
                keywords = excluded.keywords,
                description = excluded.description,
                cover = excluded.cover,
                owner = excluded.owner,
                channel = excluded.channel,
                partition = excluded.partition,
                path = excluded.path
            WHERE articles.urn = excluded.urn;
)sql";

  auto sqlCommand = sqliteService.createCommand(insertSqlText);

  auto channelsPtr = channelServer->selectChannels();
  for (const auto &chanModel : *channelsPtr) {
    auto articlesPtr =
        articleServer->scanArticles(chanModel.URN, chanModel.Path);
    for (const auto &noteModel : *articlesPtr) {
      // quark::Logger::LogInfo({noteModel.URN, noteModel.Title,
      // noteModel.Title});

      sqlCommand->BindString("$urn", noteModel.Uid);
      sqlCommand->BindString("$title", noteModel.Title);
      sqlCommand->BindString("$header", noteModel.Header);
      sqlCommand->BindString("$body", noteModel.Body);
      sqlCommand->BindString("$create_time", noteModel.CreateTime.toString());
      sqlCommand->BindString("$update_time", noteModel.UpdateTime.toString());
      sqlCommand->BindString("$creator", "");
      sqlCommand->BindString("$keywords", noteModel.Keywords);
      sqlCommand->BindString("$description", noteModel.Description);
      sqlCommand->BindString("$cover", noteModel.Cover);
      sqlCommand->BindString("$owner", "");
      sqlCommand->BindString("$channel", noteModel.Channel);
      sqlCommand->BindString("$partition", "");
      sqlCommand->BindString("$path", noteModel.Path);

      sqlCommand->Run();
      sqlCommand->Reset();
    }
  }
}

void syncAllModels() {
  // 重新创建数据库链接以避免超时
  auto targetPath = pulsar::AppConfig::Default().GetTargetPath();
  auto database_path =
      quark::JoinFilePath({quark::ResolvePath(targetPath), "polaris.sqlite"});
  auto sqliteService = quark::MTSqliteService(database_path);
  syncChannels(sqliteService);
  syncArticles(sqliteService);
}

void pulsar::runSync() {
  auto targetPath = AppConfig::Default().GetTargetPath();
  auto database_path =
      quark::JoinFilePath({quark::ResolvePath(targetPath), "polaris.sqlite"});
  auto sqliteService = quark::MTSqliteService(database_path);
  initDatabase(sqliteService);
  while (true) {
    std::cout << "runSync" << std::endl;
    std::this_thread::sleep_for(std::chrono::milliseconds(3000));
    syncAllModels();
  }
}
