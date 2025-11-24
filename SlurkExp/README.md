# SlurkExp

[![CICD](https://github.com/DeSciL/SlurkExp/actions/workflows/docker-ci.yml/badge.svg)](https://github.com/DeSciL/SlurkExp/actions/workflows/docker-ci.yml)

Treatment controller for Slurk Experiments.

### CLI

Open a powershell and run the following two commands to load the scripts:

```
iex (irm https://slurkexp.vlab.ethz.ch/webapi/slurkexp_api.ps1)
```

The powershell clients expects the following environmental variables or in a slurk-settings.json.
`SlurkEndpoint`, `SlurkApiToken`,`ExpEndpoint` ,`ExpApiKey` 

```
Connect-Slurk

Connect-Slurk -FromEnv

Connect-Slurk -Path c:\temp\slurk-settings.json
```

With the following command you can start a new room to play around with chatbots without SlurkExp's involvement.
Default BotId equals to 1, specifiy ìd with `-BotId` parameter.

```
New-SlurkExpRoom -NumUser 1 -WaitTimeout 60 -ChatTimeout 60 -Formatted
```


### Prompts

Default prompts are stored in text files under /prompts, i.e. /prompt/bot-1.txt. You can list templates with 

```
Get-SlurkExpPrompt
```

You can reset the prompt by id.

```

$p = Get-SlurkExpPrompt -PromptId 1
$p.Content = "Introduce yourself and sound like a pirate!"
$p | Update-SlurkExpPrompt
```


### Progress

You can now follow the progress of the expriment on the command line:

```
Get-SlurkExpClient -Poll
Get-SlurkExpLogEvent -Poll
Get-SlurkExpLog -Poll -Messages
```

or also via the live website:
https://slurkexp.vlab.ethz.ch/live
