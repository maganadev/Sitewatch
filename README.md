# Sitewatch

Sitewatch is a powerful tool that allows you to monitor websites for updates and changes using Puppeteer. It helps you keep track of website versions and provides detailed information about the differences between historical and current versions. The tool is designed to be highly configurable, allowing you to set up monitoring for multiple websites using simple JSON files.

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Configuration](#configuration)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Introduction

Sitewatch simplifies the process of monitoring websites for updates, making it ideal for developers, content creators, and anyone who needs to keep an eye on changes in online content. By leveraging Puppeteer, a headless browser automation library, Sitewatch can effectively simulate user interaction with websites and extract valuable information for comparison.

## Features

- **Website Monitoring:** Sitewatch can monitor multiple websites simultaneously by using separate JSON configuration files for each site.

- **Detailed Comparison:** It compares the historical version of a website with the current version to identify changes accurately.

- **Easy Configuration:** The tool is highly configurable, allowing you to define monitoring rules and other settings using simple JSON files.

- **Custom Notifications:** Get notified of website updates through custom Discord Webhooks.

- **Scheduled Scans:** Set up automated scans at regular intervals to keep track of changes over time.

## Getting Started

### Configuration

Before you can start monitoring websites, you need to set up the configuration files for each website you want to track. Configuration files are written in JSON format and contain information such as the website URL, CSS selectors for elements to track, and notification settings.

Here's an example configuration file (`PriceChange.json`) for a website:

```json
{
  "URL": "https://shop.g-******.com/products/g-******-hsk-pro-4k-wireless-mouse",
  "querySelectorQuery": ".product__price",
  "Base64_ScriptToExecute": "ZG9jdW1lbnQucXVlcnlTZWxlY3RvcigiLnByb2R1Y3RfX3ByaWNlIikuc2V0QXR0cmlidXRlKCJpZCIsInByaWNlIik7",
  "watchForPureAdditions": true,
  "watchForPureDeletions": true,
  "watchForChanges": true,
  "watchForNoChanges": false,
  "SecondsToWaitBeforeEachCheck": 3600,
  "SecondsToWaitAfterScriptExecution": 1
}
```

Create a similar JSON file for each website you want to monitor, and customize the settings according to your requirements.

Each JSON file should be put into the Tasks file that is created on first launch. Or, you can specify the folder to use by configuring the path within the `settings.json` file. 

## Usage

After adding websites to your `Tasks` folder, simply run the application to start monitoring. If your configuration was successful, you will recieve a message like this:

```
2023-08-01 14:37:50.4945|INFO|Sitewatch.Program|Adding task PriceChange
```
After Puppeteer loads the website, runs any JS scripts you have defined, and performs a querySelectorAll, you should receive this message:

```
2023-08-01 14:37:50.9307|INFO|Sitewatch.Program|Setting initial content for task PriceChange
```

Sitewatch will continue to monitor the website as defined in the configuration file, and report any changes through your defined Discord Webhook.
