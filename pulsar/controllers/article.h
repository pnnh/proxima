#pragma once

#include <pulsar/router.h>
#include <workflow/WFTaskFactory.h>

namespace pulsar {
void HandleArticles(WFHttpTask *httpTask,
                    std::shared_ptr<PLRouteContext> routeContext);
void HandleArticleGet(WFHttpTask *httpTask,
                      std::shared_ptr<PLRouteContext> routeContext);
} // namespace pulsar
