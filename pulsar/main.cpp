#include "controllers/article.h"
#include "pulsar/process.h"
#include "services/syncer/syncer.h"
#include <spdlog/spdlog.h>
#include <thread>

int main(int argc, char *argv[]) {
#ifndef NDEBUG
  spdlog::set_level(spdlog::level::info);
#endif

  constexpr int PORT = 7101;

  spdlog::info("Server started {}", PORT);

  std::thread syncer(pulsar::runSync);

  auto server = pulsar::PLServer(PORT);
  server.Reg("GET", "/articles", pulsar::HandleArticles);
  server.Reg("GET", "/articles/:uid", pulsar::HandleArticleGet);
  server.Reg("GET", "/articles/get", pulsar::HandleArticleGet);

  return server.runServer();
}
