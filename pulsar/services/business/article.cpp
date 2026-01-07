#include "article.hpp"
#include "pulsar/services/config/appconfig.h"
#include <date/date.h>
#include <pulsar/business/models/articles/article.h>
#include <quark/core/uuid/uuid.h>

#include <iostream>
#include <pqxx/pqxx>

std::vector<pulsar::PSArticleModel> pulsar::selectArticles() {
  std::vector<pulsar::PSArticleModel> articlesList;
  auto pqDsn = AppConfig::Default().GetDSN();
  try {
    pqxx::connection conn(pqDsn);
    if (conn.is_open()) {
      std::cout << "Opened database successfully: " << conn.dbname()
                << std::endl;
      const char *sqlText =
          "select uid, title, body, create_time, update_time, owner, "
          "keywords, description from articles order by update_time desc limit "
          "100;";
      pqxx::nontransaction N(conn);
      pqxx::result R(N.exec(sqlText));

      for (pqxx::result::const_iterator itr = R.begin(); itr != R.end();
           ++itr) {
        // std::cout << "Pk = " << itr[0].as<std::string>() << std::endl;
        // std::cout << "Title = " << itr[1].as<std::string>() << std::endl;
        auto model = pulsar::PSArticleModel{
            //            .title = itr[1].as<std::string>(),
            //            .body = itr[2].as<std::string>(),
            //            .create_time =
            //            quark::makeTimePoint(itr[3].as<std::string>()),
            //            .update_time =
            //            quark::makeTimePoint(itr[4].as<std::string>()),
            //            .creator = itr[5].as<std::string>()
        };
        model.Uid = itr[0].as<std::string>();
        model.Title = itr[1].as<std::string>();
        model.Body = itr[2].as<std::string>();
        // if (!itr[6].is_null()) {
        //   model.Keywords = itr[6].as<std::string>();
        // }
        // if (!itr[7].is_null()) {
        //   model.Description = itr[7].as<std::string>();
        // }

        articlesList.push_back(model);
      }
      std::cout << "Operation done successfully" << std::endl;
    } else {
      std::cout << "Can't open database" << std::endl;
      return articlesList;
    }
    // todo
    // 需要关闭链接，因为构建错误所以暂时注释。不确定是调用disconnect还是close
    // conn.disconnect();
  } catch (const std::exception &e) {
    std::cerr << e.what() << std::endl;
    return articlesList;
  }
  return articlesList;
}

std::optional<pulsar::PSArticleModel> pulsar::queryArticle(std::string uid) {
  auto pqDsn = AppConfig::Default().GetDSN();

  pqxx::connection conn(pqDsn);
  if (conn.is_open()) {
    std::cout << "Opened database successfully: " << conn.dbname() << std::endl;
    const char *sqlText =
        "select uid, title, header, body, create_time, update_time, "
        "keywords, description, status from articles "
        "where uid = $1;";
    pqxx::nontransaction N(conn);
    pqxx::result R(N.exec_params(sqlText, uid));

    for (pqxx::result::const_iterator itr = R.begin(); itr != R.end(); ++itr) {
      std::cout << "Pk = " << itr[0].as<std::string>() << std::endl;
      std::cout << "Title = " << itr[1].as<std::string>() << std::endl;
      auto model = pulsar::PSArticleModel{
          // .pk = itr[0].as<std::string>(),
          // .title = itr[1].as<std::string>(),
          // .body = itr[2].as<std::string>(),
          // .create_time = quark::makeTimePoint(itr[3].as<std::string>()),
          // .update_time = quark::makeTimePoint(itr[4].as<std::string>()),
          // .creator = itr[5].as<std::string>(),
          // //          .keywords = itr[6].as<std::string>(),
          // //          .description = itr[7].as<std::string>(),
          // .mark_lang = itr[8].as<int>(),
          // .mark_text = itr[10].as<std::string>(),
          // .status = itr[9].as<int>(),
      };
      model.Uid = itr[0].as<std::string>();
      model.Title = itr[1].as<std::string>();
      model.Header = itr[2].as<std::string>();
      model.Body = itr[3].as<std::string>();

      if (!itr[6].is_null()) {
        model.Keywords = itr[6].as<std::string>();
      }
      if (!itr[7].is_null()) {
        model.Description = itr[7].as<std::string>();
      }
      std::cout << "Operation done successfully" << std::endl;
      // todo
      // 需要关闭链接，因为构建错误所以暂时注释。不确定是调用disconnect还是close
      // conn.disconnect();
      return model;
    }
  }
  throw std::string{"Can't open database"};
}

pulsar::MessageService::MessageService()
    : connection(pulsar::AppConfig::Default().GetDSN()) {
  if (!this->connection.is_open()) {
    throw std::runtime_error("Can't open database");
  }
}

std::optional<std::vector<pulsar::PSArticleModel>>
pulsar::MessageService::selectMessages(int limit) {
  std::vector<pulsar::PSArticleModel> articlesList;
  const char *sqlText = "select uid, nid, title, header, body, keywords, "
                        "description, create_time, update_time "
                        "from posts order by update_time desc limit $1;";
  pqxx::nontransaction N(this->connection);
  pqxx::result R(N.exec_params(sqlText, limit));

  for (pqxx::result::const_iterator itr = R.begin(); itr != R.end(); ++itr) {
    auto model = pulsar::PSArticleModel{
        // .uid = itr[0].as<std::string>(),
        // .nid = itr[1].as<long>(),
        // .title = itr[2].as<std::string>(),
        // .header = itr[3].as<std::string>(),
        // .body = itr[4].as<std::string>(),
        // .keywords = itr[5].as<std::string>(),
        // .description = itr[6].as<std::string>(),
        // .create_time = quark::makeTimePoint(itr[7].as<std::string>()),
        // .update_time = quark::makeTimePoint(itr[8].as<std::string>()),
    };
    articlesList.push_back(model);
  }
  return articlesList;
}

std::optional<pulsar::PSArticleModel>
pulsar::MessageService::findMessage(const std::optional<std::string> &uid,
                                    const std::optional<long> &nid) {
  std::string sqlText = "select uid, nid, title, header, body, keywords, "
                        "description, create_time, update_time "
                        "from posts where ";

  std::string pk;
  if (uid != std::nullopt && quark::MTUUID::isUUID(uid.value())) {
    sqlText += " uid = $1;";
    pk = uid.value();
  } else if (nid != std::nullopt) {
    sqlText += " nid = $1;";
    pk = std::to_string(nid.value());
  } else {
    return std::nullopt;
  }

  pqxx::nontransaction N(this->connection);
  pqxx::result R(N.exec_params(sqlText, pk));

  for (pqxx::result::const_iterator itr = R.begin(); itr != R.end();) {
    auto model = pulsar::PSArticleModel{
        // .uid = itr[0].as<std::string>(),
        // .nid = itr[1].as<long>(),
        // .title = itr[2].as<std::string>(),
        // .header = itr[3].as<std::string>(),
        // .body = itr[4].as<std::string>(),
        // .keywords = itr[5].as<std::string>(),
        // .description = itr[6].as<std::string>(),
        // .create_time = makeTimePoint(itr[7].as<std::string>()),
        // .update_time = makeTimePoint(itr[8].as<std::string>()),
    };
    return model;
  }
  return std::nullopt;
}

int pulsar::MessageService::insertMessage(const pulsar::PSArticleModel &model) {
  const char *sqlText =
      "insert into messages (uid, title, content, create_time, update_time, "
      "creator, sender, receiver) "
      "values ($1, $2, $3, to_timestamp($4, 'YYYY-MM-DDTHH24:MI:SS.USZ'), "
      "to_timestamp($5, 'YYYY-MM-DDTHH24:MI:SS.USZ'), $6, $7, $8);";

  pqxx::work W(this->connection);
  auto createTime = date::format(
      "%FT%TZ", std::chrono::time_point_cast<std::chrono::microseconds>(
                    model.CreateTime.toTimePoint()));
  auto updateTime = date::format(
      "%FT%TZ", std::chrono::time_point_cast<std::chrono::microseconds>(
                    model.UpdateTime.toTimePoint()));
  W.exec_params(sqlText, model.Uid, model.Title, model.Body, createTime,
                updateTime);
  W.commit();

  return 0;
}

int pulsar::MessageService::updateMessage(const pulsar::PSArticleModel &model) {
  const char *sqlText = "update messages set title = $1, content = $2, "
                        "update_time = $3, sender = $4, receiver = $5 "
                        "where uid = $6;";

  pqxx::work W(this->connection);
  W.exec_params(sqlText, model.Title, model.Body, 12344443232, model.Title,
                model.Title, model.Uid);
  W.commit();

  return 0;
}

int pulsar::MessageService::deleteMessage(const std::string &uid) {
  const char *sqlText = "delete from messages where uid = $1;";

  pqxx::work W(this->connection);
  W.exec_params(sqlText, uid);
  W.commit();

  return 0;
}

long pulsar::MessageService::count() {
  const char *sqlText = "select count(*) from messages;";
  pqxx::nontransaction N(this->connection);
  pqxx::result R(N.exec(sqlText));

  for (pqxx::result::const_iterator itr = R.begin(); itr != R.end();) {
    return itr[0].as<long>();
  }
  throw std::runtime_error("Can't get count");
}
