
#include "channel.h"
#include <string>
#include <filesystem>
#include <utils/basex.h>

#include "types/Exception.h"
#include "services/filesystem/filesystem.h"
#include "services/yaml/yaml.h"
#include "utils/md5.h"
#include "types//String.h"

quark::ChannelServerBusiness::ChannelServerBusiness(const std::string& baseUrl)
{
    this->baseUrl = baseUrl;
}

std::shared_ptr<std::vector<quark::PSChannelModel>>
quark::ChannelServerBusiness::selectChannels() const
{
    auto channels = std::make_shared<std::vector<PSChannelModel>>();

    for (const auto& entry : std::filesystem::directory_iterator(this->baseUrl))
    {
        auto dirName = entry.path().filename();
        if (entry.path() == "." || entry.path() == ".." || !entry.is_directory())
        {
            continue;
        }

        auto filePath = dirName.string();
        if (!isChannelDirectory(filePath))
        {
            continue;
        }
        auto channelModel = quark::PSChannelModel(filePath);
        auto metadataFilePath = quark::JoinFilePath({this->baseUrl, filePath, "metadata.yaml"});
        if (quark::IsFileExist(metadataFilePath))
        {
            auto yamlHandler = quark::YamlHandler(metadataFilePath);
            channelModel.URN = quark::PSString::toLower(yamlHandler.getString("metadata.urn").value_or(""));
            channelModel.Title = yamlHandler.getString("metadata.title").value_or(filePath);
            channelModel.Description = yamlHandler.getString("metadata.description").value_or("");
            channelModel.Image = yamlHandler.getString("metadata.image").value_or("");
        }
        channelModel.Path = entry.path().string();
        if (channelModel.URN.empty())
        {
            channelModel.URN = quark::PSString::toLower(quark::calcMd5(entry.path().string()));
        }
        channels->emplace_back(channelModel);
    }

    return channels;
}

bool quark::isChannelDirectory(const std::string& directoryName)
{
    return quark::PSString::EndsWith(directoryName, ".chan") ||
        quark::PSString::EndsWith(directoryName, ".channel") || quark::PSString::EndsWith(
            directoryName, ".notechannel");
}
