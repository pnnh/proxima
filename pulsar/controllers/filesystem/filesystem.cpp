#include "pulsar/controllers/filesystem/filesystem.h"
#include "quark/build.h"#include "quark/services/filesystem/filesystem.h"

#include "quark/business/filesystem/file.h"
#include "quark/core/types/string.h"
#include "quark/infra/http/query.h"
#include "quark/infra/utils/basex.h"

#include <iostream>
#include <nlohmann/json.hpp>
#include <spdlog/spdlog.h>
#include <workflow/HttpMessage.h>


using json = nlohmann::json;

void pulsar::HandleFileList(WFHttpTask *httpTask) {
  protocol::HttpRequest *request = httpTask->get_req();
  protocol::HttpResponse *response = httpTask->get_resp();

  response->set_http_version("HTTP/1.1");
  response->add_header_pair("Content-Type", "application/json; charset=utf-8");
  response->add_header_pair("Access-Control-Allow-Origin", "*");

  auto request_uri = request->get_request_uri();

  quark::MTQueryString queryParam{std::string(request_uri)};

  auto homeDirectory = quark::UserHomeDirectory();
  auto baseUrl = homeDirectory;
  auto encodePath = queryParam.getString("path");
  if (encodePath.has_value() && !encodePath.value().empty()) {
    auto decodePath = quark::decode64(encodePath.value());
    baseUrl = quark::MTString::LeftReplace(decodePath, "~", homeDirectory);
  }

  std::ostringstream oss;
  auto fileServer = std::make_shared<quark::FileServerBusiness>();
  auto filesPtr = fileServer->selectFilesVector(baseUrl);
  json range = json::array();
  for (const auto &model : filesPtr) {
    json item = {
        {"URN", model.URN},
        {"Title", model.Title},
        {"Name", model.Name},
        {"Description", model.Description},
        {"IsDir", model.IsDir},
        {"IsHidden", model.IsHidden},
        {"IsIgnore", model.IsIgnore},
        {"CreateTime", model.CreateTime.toString()},
        {"UpdateTime", model.UpdateTime.toString()},
    };
    range.push_back(item);
  }

  auto count = filesPtr.size();
  json data = json::object({
      {"code", 200},
      {"message", "Hello, World!"},
      {
          "data", {
              {"count", count},
              {
                  "range",
                  range,
              }
          }
      },
  });
  oss << data;

  auto bodyStr = oss.str();
  auto bodySize = bodyStr.size();

  response->append_output_body(bodyStr
                               .c_str(), bodySize);

  response->set_status_code(
      "200"
      );
}
