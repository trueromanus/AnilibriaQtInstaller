#!/usr/bin/env ruby

# frozen_string_literal: true

require 'json'
require 'tmpdir'
require 'open-uri'

def semver_gt(v1, v2)
  v1[1].to_i>v2[1].to_i ||
    v1[2].to_i>v2[2].to_i ||
      v1[3].to_i>v2[3].to_i ||
        v1[4].to_i>v2[4].to_i ||
          v1[5].to_i>v2[5].to_i
end

SEMVER_REGEXP = /(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?/

json = JSON.parse(URI.open('https://api.github.com/repos/anilibria/anilibria-winmaclinux/releases',
                           'Accept' => 'application/vnd.github+json',
                           'X-GitHub-Api-Version' => '2022-11-28',
                           'User-Agent' => 'curl/7.88.1').read)

last_release = json[0]
last_release_asset = last_release['assets'].filter do |e|
  /^.*#{Regexp.quote(RUBY_PLATFORM.split('-')[0])}.*\.flatpak$/i.match?(e['name'])
end[0]

if last_release_asset.nil?
  warn "FERR: flatpak asset for #{RUBY_PLATFORM.split('-')[0]} not found"
  exit 2
end

update = false

installed_version = IO.popen('flatpak list').read.split("\n").filter { |e| /anilibria/i.match?(e) }
if installed_version.empty?
  warn 'INFO: No installed version found, installing'
  update = true
elsif SEMVER_REGEXP.match?(installed_version[0])
  local_ver = SEMVER_REGEXP.match(installed_version[0])
  remote_ver = SEMVER_REGEXP.match(last_release['tag_name'])
  warn "INFO: Found version #{local_ver}, remote version is #{remote_ver}"
  if semver_gt(remote_ver, local_ver)
    warn 'INFO: Updating'
    update = true
  else
    warn 'INFO: Already last version'
  end
else
  # TODO: add checks?
  warn 'WARN: Can\'t find any info about installation status, trying to install/update'
  update = true
end

if update
  Dir.mktmpdir do |_d|
    warn 'INFO: Downloading update'
    update = URI.parse(last_release_asset['browser_download_url']).open('User-Agent' => 'curl/7.88.1')
    f = File.open(last_release_asset['name'], 'wb')
    f.sync = true
    f.write(update.read)
    f.fsync
    f.close
    warn 'INFO: Trying to install'
    install_cmd = 'flatpak install --noninteractive --or-update '
    unless system("#{install_cmd} ./#{last_release_asset['name']}")
      warn 'ERR: Default installation method failed, trying user installation'
      unless system("#{install_cmd} --user ./#{last_release_asset['name']}")
        warn 'FERR: User installation failed'
        exit 1
      end
    end
  end
end
