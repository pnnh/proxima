#pragma once

#include "location_service.h"

class SyncService {
public:
  void SyncLibraries();
  int SyncImages(const QString &path);

private:
  proxima::LocationService libraryService;
};
