# HostsSwitch

Introduction 
============

"hosts" file is probably one of the most known configuration files of Windows operation system. 
It maps hostname to IP address as [Wiki describes](https://en.wikipedia.org/wiki/Hosts_(file)). 

Use of "hosts" file is a simple way to substitute access to one server with another one temporarily, 
either for development or test purposes.  

If you are Windows developer, you probably familiar with procedure of editing Windows 
"hosts" file like: 

1.  Go to directory c:\windows\system32\drivers\etc and find the file named "hosts".

2.  Open the file for edit …

...

7.  Run command "ipconfig /flushdns" using CMD to update DNS according new content of "hosts" file.

Well, if you do it often, you probably get tired eventually and write your own script to do the job.

It is all well and good until you start working with two or more servers and two or more 
environments. Number of scripts grows and developer starts having difficult time to remember 
which "hosts" file is active on which server at present time. 

When that happen, a tool designated to watch and manage "hosts" files on multiple computers comes 
in handy. 

Let’s introduce Hosts Switch desktop application, which does exactly that; it copies predefined 
"hosts" file and flushes DNS at the touch of a button, as well as it shows content of active 
"hosts" file of each computer. 

##### Note: HostsSwitch C# project can be opened in Visual Studio 2017, targeting .NET framework 4.7.

How it works
============

How to configure the application -> see Configuring environments section below. 

When you run the application, you can see loaded configured environments as it is shown on the 
screenshot below: 

![HostsSwitch Project](/Images/hosts2a.jpg)

Select an Environment; "SRV-IIS-DEV local VM" is shown as selected above. 

Text area below "current environment" shows content of Effective Hosts File (see definition below), 
and hostfiles dropdown list shows name of Effective Hosts File.

Click on the dropdown list to see all Named Host Files (see definition below). 

Select one of the options, "DEV env" for example: 

![HostsSwitch Project](/Images/hosts2.jpg)

If you click Undo button, current selection will be rolled back to original host file.

If you click Commit button, currently selected Named Host File will be copied to "\etc" directory 
of the computer; also DNS will be flushed automatically. In the example above, "DEV env" hosts file 
will be copied to "\\SRV-IIS-DEV\etc" shared directory, which actually is "c:\windows\system32\drivers\etc" 
directory on SRV-IIS-DEV computer, followed by execution of "ipconfig /flushdns" command on that remote computer.

Definitions
============

Before we start reviewing application features, let’s agree on definitions: 

1.  Environment – basically it means a computer. In our terms, it represents collection of predefined 
    "hosts" files of computer alone with the environment's descriptor file "env.config". Physically, it is a directory, 
	where the "env.config " as well as all Named Host Files (see next definition) of the computer are located. 

2.  Named Host File – a predefined custom "hosts" file, which is configured for specific Environment 
    (see above). It may contain multiple hostname / IP address pairs as 
	[Wiki describes](https://en.wikipedia.org/wiki/Hosts_(file)).

3.  Effective Hosts File – a Named Host File, which is currently present in "c:\windows\system32\drivers\etc" 
    directory. 

4.  Repo – a directory - repository of all named Environments and evidently all respective Named Host Files.

Here’s an example of Repo where 2 Environments are configured: "localhost" and "SRV-IIS-DEV". 

![HostsSwitch Project](/Images/repo2.jpg)

As you see, Environment of computer "SRV-IIS-DEV" has 4 predefined Named Host Files, which could be 
configured as next: 

-   "0.Local_only_no_mapping" – has no substitutions; "localhost" of DEV computer as is 

-   "1.Dev_env" - resolves to point a Database name to a DB on a DEV environment computer

-   "2.Test_env" - resolves to point a Database name to a DB on a TEST environment computer

-   "3.Stage_env" - resolves to point a Database name to a DB on a STAGE environment computer

This configuration would result in creation of 4 options of "hosts" files in dropdown list, 
as it is shown below: 

![HostsSwitch Project](/Images/hosts2a.jpg)

Application description
=======================

Hosts Switch is WPF application and it has 3 main section. Section one is designed to show all configured 
Environments, where user can see or manage configuration of "hosts" files of all computers in one place. 

Other sections provide basic Help and application Settings management. 

### Environments 

Click on: computer icon

Each environment has its own designated Tab, where user can see and manage "hosts" files of all 
configured computers, as it is shown below: 

![HostsSwitch Project](/Images/hosts1.jpg)

![HostsSwitch Project](/Images/hosts2.jpg)

It may take time to load current content of "hosts" file of remote computer. Therefore, you may see 
progress bar showing current state of loading process of Environments. 

![HostsSwitch Project](/Images/hosts1e.jpg)

If computer represented by an Environment is offline, not accessible or misconfigured, Tab of 
respective Environment would show error state of that Environment, as it is shown on screenshot below: 

![HostsSwitch Project](/Images/hosts2e.jpg)

### Settings

Click on: tools like icon

This section provides basic management of the app settings. Application log is the only configured 
feature so far. Here you can clear log immediately, set clearing logs flag to erase all logs on 
Refresh button click, as well as select logging level to:

1.  all – log every click

2.  results and errors – log only important actions as well as errors

3.  errors only – log errors only

![HostsSwitch Project](/Images/hosts1c.jpg)

![HostsSwitch Project](/Images/hosts2c.jpg)

### Help

Click on: open book icon

This section contains basic reminder that this is open source project under Apache license.

![HostsSwitch Project](/Images/hosts1h.jpg)

Configuring environments
========================

Let’s configure Environments in Repo.

First, you need to create new directory in repo, and give it a meaningful name; name of computer would be the best.

![HostsSwitch Project](/Images/repo.jpg)

#### env.config 

As you can see, each environment (localhost and SRV-IIS-DEV on the picture above), 
has "env.config" file: 

```
<?xml version="1.0" encoding="utf-8" ?>
<Env>
	<Title>localhost</Title>
	<EnvHost>localhost</EnvHost>
	<EnvHostPath>c:\Windows\System32\drivers\etc</EnvHostPath>
</Env>
```

and another one: 

```
<?xml version="1.0" encoding="utf-8" ?>
<Env>
	<Title>SRV-IIS-DEV local VM</Title>
	<EnvHost>SRV-IIS-DEV</EnvHost>
	<EnvHostPath>\\SRV-IIS-DEV\etc</EnvHostPath>
</Env>
```
You need to create new "env.config" file in the directory of that new Environment. Add the XML above to the file 
and configure its elements:

| Element     | Description                                                                                      |
|-------------|--------------------------------------------------------------------------------------------------|
| Title       | Text of title to show on Environment’s Tab                                                       |
| EnvHost     | Name or IP address of computer                                                                   |
| EnvHostPath | Local or shared directory where "hosts" file is located on local or remote computer respectively |

#### Named Host Files

In order to complete configuration, you need to add new Named Host Files to the Environment.
It is important to name the files properly. Template of name of file of Named Hosts File is:

singledigit.name_of_file

The file name does not allow spaces. Digit must start with 0. Next Named Hosts Files must 
start with single digit incremented by one, etc. Underscores will be replaced by spaces 
as it is shown on screenshot below: 

![HostsSwitch Project](/Images/hosts2a.jpg)

License
=======

This project, along with any associated source code and files, is licensed under
Apache License 2.0.
