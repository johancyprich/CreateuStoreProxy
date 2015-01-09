**CreateuStoreProxy**
==============

Written by Johan Cyprich (jcyprich@live.com)<br />
Version: 1.0.0<br />
December 23, 2014<br />
License: The MIT License (MIT)


**REQUIREMENTS:**
--------------
Windowx XP, Vista, 7, 8, 2008, 2012
.NET 4.0

**DESCRIPTION:**
--------------
Creates the httpd.conf for URL redirection by the Helicon software on the XMPie uStore proxy server. The 
file is usually found at Program Files\Helicon\ISAPI_Rewrite3. CreateuStoreProxy is a .NET 4 application
which can be run on any computer that has file access to the proxy server. You may need to run the following
net use command to be able to access the server if its not directly available to you:

    net use \\192.168.149.33\c$ password /user:administrator

Enter the correct IP of the proxy server and the password for the admin account to get access to drive C:,
then this app will be able to modify the httpd.conf file. You can optionally close the connection to the proxy
server with:

    net use \\192.168.149.33\c$ /delete

This program needed to be created when the code to generate the proxy by the uStore Admin stopped working
due to line 17 being too large for VBS to execute. Line 17 has a string which contains all of the URL 
redirect info for building httpd.conf. Normally, this would work since most uStore have a small number of
stores, but when you reach 180+ stores the line becomes too large.

CreateuStoreProxy is a command line which can be run from the Command Shell, Windows PowerShell, or the Run
command from the Start menu (press Windows-R).

The appliation will copy the new httpd.conf (saved in the application folder) to the uStore server. The
existing .conf file will be renamed before the new file is copied to the folder. The old file will have the 
current date (YYYYMMDD) appended to its filename.
