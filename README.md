# CATSV3
## Table of Contents
1. [General Info](#general-info)
2. [Technologies](#technologies)
3. [Installation](#installation)
4. [Collaboration](#collaboration)
5. [Roles](#faqs)

### General Info
***
CATS is an automated workflow system that encourages a collaborative environment, enables the tracking of 
documents as they go through a comprehensive review and clearance process, and serves as a repository of 
correspondences   and other official documents 

## Technologies
***

* Front End is Angular: Version 11 
* Middle Layer is .NetCore: Version 3.1
* Data layer:
	Database is SQL 2019 : Version 15.0.4123.1
	Sharepoint: Version 2019
	
## Installation
***
Client side installation. 
$ git clone "CATS V3" project
$ cd ..CATSV3\Client\CatsV3
$ npm install

Database installation. 
Go to ...CATSV3\Documents folder.
1. Create a CATS database such as "Database Name."
2. Open the file "CATSV3 Database Creation" and run the script in SQL Management Studio: This will create the database and insert the initial data in several tables
3. Please make sure your IIS Application Pool service account/your local user account is granted Select and Insert permission in the SQL Database instance
4. Run the script file "Set CATS Administrators" to insert the initial CATS administrators accounts. Please make sure your account is added.

Sharepoint.
3. Create the Sharepoint Library "CATS Documents Library" in your Sharepoint site instance
4. Please make sure your IIS Application Pool service account/your local user account has the Sharepoint site administrator role

IIS.
1. Please make sure both the Client site and Web API Application IIS site are both using the same above Application pool
2. The angular Client is pushed to the main CATS site
3. Create a Web Application under the main CATS site and give it the name "api": using the same app pool as the above
4. Deploy the Web API in the above "api" physical path
5. .NetCore Web API Deployment Mode: Self-Contained: if.NetCore 3.1 is not already in the webserver
6. Make sure the application pool account is given read/write permission to both the Client and application "API" physical path folders

Other.
1. Update the .Netcore Web API "web.config" in the environmentVariables node:
	1. MOD.CatsV3.ConnectionString , value="Server=Instance;Database=YOUR DATABASE;Trusted_Connection=True;MultipleActiveResultSets=true" : This is your CATS DB connection string
	2. MOD_User_ConnectionString, value="Server=Instance;Database=YOUR DATABASE;Trusted_Connection=True;MultipleActiveResultSets=true" : This is your DB where the employee table is located
	3. MOD.CatsV3.SiteUrl, value="Your CATS Client site name: IIS: https://sitename"
	4. MOD.CatsV3.SiteAPIUrl, value="Your Web API application: https://sitename/api"
	5. MOD.CatsV3.SPSiteUrl, value="Your Sharepoint site where the document library is created: https://sharepoint/sites/cats"
	6. MOD.CatsV3.document library, value ="Sharepoint Document library name": Created in the Sharepoint step 5.
	7. MOD.CatsV3.CATSArchiveEmailService, value="mailbox to archive the final package emails": for preservation of presidential record purposes
	
Dependencies:
1. Must have the Employee table where to find the users. This database connection string is set at MOD_User_ConnectionString (step 2. above)
2. Must have the Framework (submodule) project underneath CATSV3 folder as such ...CATSV3\Framework
3. CATS uses the users' "UPN" as the unique user account id. 

## Collaboration
***
Please contact support.

## Roles
Check table Roles

