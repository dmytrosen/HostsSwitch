# HostsSwitch

Introduction 
============

"hosts" file is probably one of the most known configuration files of Windows operation system. 
As [Wiki](https://en.wikipedia.org/wiki/Hosts_(file)) describes, it maps hostname to IP address. 

Use of "hosts" file is simple way to temporary substitute one server with another one, either 
for development or test purposes.  

If you are Windows developer, you probably familiar with procedure of editing Windows 
"hosts" file like: 

1.  Go to directory c:\windows\system32\drivers\etc and find the file named "hosts".

2.  Open the file for edit …

...

7.  Run command "ipconfig /flushdns" using CMD to update DNS according new content of "hosts" file.

Well, if you do it often, you probably get tired eventually and write your own script to do the job.

It is all well and good until you start working with two or more servers and two or more 
environments. Number of scripts grows, and developer starts having difficult time to remember 
which substitution "hosts" file is active of which server at present time. 

In such situation, a tool designated to watch and manage "hosts" files on multiple computers comes 
in handy. Let’s introduce Hosts Switch desktop application, which does exactly that; it replaces 
"hosts" files with selected predefined ones as well as flushes DNS and shows contents of active 
"hosts" files on multiple computers. 

How it works
============

Configure the application, see Configuring environments section below. 

Run the application; you should see you configured environments loaded as it is shown on 
example below: 

![HostsSwitch Project](/Images/hosts2a.jpg)

Select an Environment; SRV-IIS-DEV local VM is selected on example above. 

Text area below "current environment" shows content of Effective Hosts File (see definition below), 
and hostfiles dropdown list shows name of Effective Hosts File.

Click on the dropdown list to see all Named Host Files (see definition below). 

Select one of the options, DEV env for example: 

![HostsSwitch Project](/Images/hosts2.jpg)

If you click on Undo button, selection will be rolled back to original host file.

If you click on Commit button, currently selected Named Host File will be copied to "etc" directory 
of the computer, also DNS will be flushed automatically. In the example above, "DEV env" hosts file 
will be copied to \\SRV-IIS-DEV\etc shared directory, which is c:\windows\system32\drivers\etc directory 
on SRV-IIS-DEV computer following by executing "ipconfig /flushdns" command on that remote computer.

Definitions
============

Before we start reviewing application features, let’s agree on some definitions. 

1.  Environment – a computer setting; represents collection of predefined "hosts" files of computer. 
    It is directory, where Named Host Files (see next) of the computer are located. 

2.  Named Host File – a predefined custom "hosts" file, which is configured for specific Environment 
    (see above). 

3.  Effective Hosts File – a Named Host File, which currently in c:\windows\system32\drivers\etc 
    directory. 

4.  Repo – repository of named Environments and evidently all respective Named Host Files.

Here’s an example of Repo where 2 Environments are configured: localhost and SRV-IIS-DEV. 

![HostsSwitch Project](/Images/repo2.jpg)

As you see, Environment of computer SRV-IIS-DEV has 4 predefined Named Host Files, which may have 
been configured to: 

-   0.Local_only_no_mapping – has no substitutions; "localhost" of DEV computer as is 

-   1.Dev_env - point a Database to a DB on a DEV environment computer

-   2.Test_env - point a Database to a DB on a TEST environment computer

-   3.Stage_env - point a Database to a DB on a STAGE environment computer

This configuration would result in creating 4 options of "hosts" files selection in dropdown list, 
as it is shown below: 

![HostsSwitch Project](/Images/hosts2a.jpg)

Application description
=======================

Hosts Switch WPF application has 3 main section. Section one is designed to show all configured 
Environments, where user can see or manage all configurations in one place. 

Other sections provide basic Help and application Settings management. 

### Environments 

Click on: computer icon

Each environment has its own designated Tab, where user can see and manage "hosts" files of all 
configured computers, as it is shown below: 

![HostsSwitch Project](/Images/hosts1.jpg)

![HostsSwitch Project](/Images/hosts2.jpg)

It may take time to load current content of "hosts" file of remote computer. So, you may see 
progress bar showing state of loading process of Environments. 

![HostsSwitch Project](/Images/hosts1e.jpg)

If computer configured by an Environment is offline, not accessible or misconfigured, Tab of 
respective Environment would show error state of that Environment. See example below: 

![HostsSwitch Project](/Images/hosts2e.jpg)

### Settings

Click on: tools like icon

This section provides basic management of the app settings. Application log is the only configured 
feature so far. Here you can clear log immediately, set clearing logs flag to erase all logs when 
environments refresh button is clicked, as well as select logging level to:

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

where: 

| Element     | Description                                                                                      |
|-------------|--------------------------------------------------------------------------------------------------|
| Title       | Text of title to show on Environment’s Tab                                                       |
| EnvHost     | Name or IP address of computer                                                                   |
| EnvHostPath | Local or shared directory where "hosts" file is located on local or remote computer respectively |

#### Named Host Files

All files except env.config in Environment directory are Named Hosts Files. It is important to name 
the files properly. Template of Named Hosts Files is:

single digit.name_of_file

The file name does not allow spaces and digit must start with 0. Next Named Hosts Files must 
start with single digit incremented by one, etc. Underscores will be replaced by spaces when 
are shown on drop down list of Named Hosts Files of Environment as it is on picture below: 

![HostsSwitch Project](/Images/hosts2a.jpg)

License
=======

This project, along with any associated source code and files, is licensed under
Apache License 2.0.
