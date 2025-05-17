#include "pulsar/process.h"

#include <iostream>

#include "router.h"
#include <spdlog/spdlog.h>
#include <stdlib.h>
#include <string>
#include <workflow/WFHttpServer.h>

void pulsar::PLServer::process(WFHttpTask *httpTask) {
  protocol::HttpRequest *request = httpTask->get_req();
  std::string request_uri = request->get_request_uri();
  std::string request_method = request->get_method();

  try {
    this->router.Route(httpTask);
  } catch (const std::exception &exception) {
    std::cerr << "[" << request_method << " " << request_uri
              << "] server exception: " << exception.what() << std::endl;
    protocol::HttpResponse *response = httpTask->get_resp();
    response->set_status_code("500");
  }
}

void loopCmd() {
  std::string cmd;
  while (true) {
    std::cout << "等待操作命令..." << std::endl;
    std::getline(std::cin, cmd);
    std::cin.clear();
    if (cmd == "exit") {
      break;
    }
  }
}

pulsar::PLServer::PLServer(int port) : port(port) {
  this->httpServer = std::make_unique<WFHttpServer>(
      [this](WFHttpTask *httpTask) { this->process(httpTask); });
}

int pulsar::PLServer::runServer() {
  if (port <= 0) {
    constexpr int defaultPort = 8501;
    port = defaultPort;
  }

  if (this->httpServer->start(port) == 0) {
    loopCmd();
    this->httpServer->stop();
  } else {
    perror("server start failed");
    exit(1);
  }
  return 0;
}

void pulsar::PLServer::Reg(
    std::string method, const std::string &path,
    void (*handler)(WFHttpTask *,
                    std::shared_ptr<PLRouteContext> routeContext)) {
  this->router.Reg(std::move(method), path, handler);
}
