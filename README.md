# Morestachio-EmailProcessor
A Simple Mass E-Mail Processor.

The Ui allows an end-user to be guided trough the steps: Data Import, Templating, Mail Distributor and sending.

## Prerequisites

- The Mail Processor needs .net 5.0 installed

## Steps

### Data Acquisition 

With the current application you can choose between 2 import methods:

![image](https://user-images.githubusercontent.com/6794763/109884519-68722780-7c7d-11eb-93fe-d64b997250e2.png)

Depending on what Data Source you have selected, the next step will be different.   

> All direct next steps will require you to validate your settings before you can continue!

#### CSV Import

You have to add the path to a CSV file (using ; as a seperator)

![image](https://user-images.githubusercontent.com/6794763/109884697-acfdc300-7c7d-11eb-9f8f-8020674b9c2c.png)

#### SQL Import
You can also import your data from an MS-SQL server using a connection string and a Query.

![image](https://user-images.githubusercontent.com/6794763/109887188-95283e00-7c81-11eb-97e9-4c305866276b.png)


### Set Mail Properties
To generate a mail, you have to enter several paths to where certain properties of the mail like the Receivers Address & Name, the Mails subject and the Senders Address & Name is located in your data. To the right of your screen you can see the Data's structur. If you want to set constant values you have to enclose your values with `"` like:    

From Address Expression: `"newsletter@test.com"`   

![image](https://user-images.githubusercontent.com/6794763/109887466-0536c400-7c82-11eb-86b6-81b665246284.png)

### Template
To generate an individial email for your sendout you can write a HTML morestachio template that will be used to generate the mail body for each mail.

![image](https://user-images.githubusercontent.com/6794763/109885549-f7cc0a80-7c7e-11eb-9fc1-1146e2aa2a4a.png)

### Data Sendout
You can send your generated mails to ether a SMTP server or save them as a `.mime` document.   

> All direct next steps will require you to validate your settings before you can continue!   
 
### Review and Sendout
When everything is entered, you can review your settings and choose to use a parallel sendout. When using the parallel method you can set the amount of threads and buffered mails in memory.

![image](https://user-images.githubusercontent.com/6794763/109885748-5db89200-7c7f-11eb-95bc-b8132ff9c875.png)

When finished, you can ether rest the UI to repeat the operation or generate a "Send Report" containing all failed to send mails.

![image](https://user-images.githubusercontent.com/6794763/109886033-c56edd00-7c7f-11eb-8ef2-9182e6e501a7.png)

