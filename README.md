# SFA.DAS.EmployerFinance

[![Build status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/Manage%20Apprenticeships/das-employerfinance)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=1212)

## Requirements

1. Install [.NET Core].
2. Install [Docker].
3. Install [Node].
4. Install [Npm].
5. Install [Gulp].

### Windows

Run the following PowerShell commands in the `tools` directory, [Choclatey] will also be installed:

```powershell
> iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))
> choco install dotnetcore-sdk
> choco install docker-desktop
> docker-compose up -d
> choco install nodejs
> npm install -g npm
> npm install -g gulp
```

### macOS

Run the following Bash commands in the `tools` directory, [Homebrew] will also be installed:

```bash
$ /usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
$ brew tap caskroom/cask
$ brew cask install dotnet-sdk
$ brew cask install docker
$ docker-compose up -d
$ brew install nodejs
$ npm install -g npm
$ npm install -g gulp
```

## Setup

### Add configuration

* Clone the [das-employer-config](https://github.com/SkillsFundingAgency/das-employer-config) repository.
* Clone the [das-employer-config-updater](https://github.com/SkillsFundingAgency/das-employer-config-updater) repository.
* Run in the `das-employer-config-updater` directory:

```powershell
> dotnet run
```

* Follow the instructions to import the config from the directory that you cloned the `das-employer-config` repository to.

> The two repositories above are private. If the links appear to be dead, make sure that you're logged into GitHub with an account that has access to these i.e. that you are part of the Skills Funding Agency Team organization.

### Add database

Run in the `src/SFA.DAS.EmployerFinance.Jobs` directory:

```powershell
> dotnet run
```

* Wait until you see the following in the shell window and then press Ctrl + C:
  * Finished deploying database
  * Executed 'DeployDatabaseJob.Run'

### Add packages

Run in the `src/SFA.DAS.EmployerFinance.Web` directory:

```powershell
> npm install
```

### Add certificates

```powershell
> dotnet dev-certs https --trust
```

## Run

Run in the `src/SFA.DAS.EmployerFinance.Web` directory:

```powershell
> dotnet run
```

Alternatively:

* Open `SFA.DAS.EmployerFinance.sln` in your IDE e.g. [Rider], [Visual Studio], [Visual Studio Code] etc.
* Start debugging the `SFA.DAS.EmployerFinance.Web` project.

## Test

Run in the `src` directory:

```powershell
> dotnet test
```

## Gulp tasks

Run in the `src/SFA.DAS.EmployerFinance.Web` directory:

```powershell
> gulp <task>
```

|Task|Description|
|----|-----------|
|`default`|Runs the `css` & `js` tasks. |
|`css`|Compiles all scss files in `content/styles` to `wwwroot/css/das.css`.|
|`js`|Copies all js files in `node_modules/govuk-frontend` to `content/javascript/govuk-frontend`.|

[.NET Core]: https://dotnet.microsoft.com/download
[Azure Storage Explorer]: http://storageexplorer.com
[Azurite]: https://github.com/azure/azurite
[Choclatey]: https://chocolatey.org
[Docker]: https://www.docker.com
[Git]: https://git-scm.com
[Gulp]: http://gulpjs.com
[Homebrew]: https://brew.sh
[Node]: http://nodejs.org
[Npm]: https://www.npmjs.com/package/npm
[Rider]: https://www.jetbrains.com/rider
[SQL Server]: https://www.microsoft.com/en-us/sql-server/sql-server-2017
[Visual Studio]: https://www.visualstudio.com
[Visual Studio Code]: https://code.visualstudio.com