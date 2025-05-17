#pragma once
#include "quark/business/models/channel/mail.hpp"

#include <optional>
#include <vector>
#include <pqxx/pqxx>

class MailService {
public:
  MailService();

  ~MailService();

  std::optional<std::vector<MTMailModel>> selectMails(int limit);

  int insertMail(const MTMailModel &model);

  int updateMail(const MTMailModel &model);

  int deleteMail(const std::string &pk);

  std::optional<MTMailModel> findMail(const std::string &pk);

  long count();

private:
  pqxx::connection connection;
};
