# Morestachio-EmailProcessor
A Simple Mass E-Mail Processor.

The Ui allows an end-user to be guided trough the steps: Data Import, Templating, Mail Distributor and sending.

## Prerequisites

- The Mail Processor needs .net 5.0 installed

## Start
When starting the application you can ether choose to load an existing configuration or start fresh by clicking the "next" button to the lower right.   
There is also an indicator whenever WebView2 is supported. That should be the case on Windows 10, Windows 8 and Windows 7 computers.

![image](https://user-images.githubusercontent.com/6794763/110017615-b1ca8180-7d26-11eb-96a4-19bbc2ca7748.png)

During the whole process you can, at each step chose to save all entered data by clicking the "save" button to the upper right. This will allow you to pre-fill all data you already have entered when starting a new process.

## Steps

### Data Acquisition 

With the current application you can choose between 2 import methods:

![image](https://user-images.githubusercontent.com/6794763/110017815-e9392e00-7d26-11eb-886a-3e51b4969d27.png)

Depending on what Data Source you have selected, the next step will be different.   

> All direct next steps will require you to validate your settings before you can continue!
> ![image](https://user-images.githubusercontent.com/6794763/110017922-0837c000-7d27-11eb-851f-f446474a38a7.png)


#### CSV Import

You have to add the path to a CSV file (using ; as a seperator)

![image](https://user-images.githubusercontent.com/6794763/110017837-f0603c00-7d26-11eb-955e-284edbd6cdb9.png)

#### SQL Import
You can also import your data from an MS-SQL server using a connection string and a Query.

![image](https://user-images.githubusercontent.com/6794763/110017860-f7874a00-7d26-11eb-88e9-e899c51b842e.png)

### Set Mail Properties
To generate a mail, you have to enter several paths to where certain properties of the mail like the Receivers Address & Name, the Mails subject and the Senders Address & Name is located in your data. To the right of your screen you can see the Data's structur. If you want to set constant values you have to enclose your values with `"` like:    

From Address Expression: `"newsletter@test.com"`   

![image](https://user-images.githubusercontent.com/6794763/110017958-1554af00-7d27-11eb-8ccb-e35959c77cb0.png)

### Template
To generate an individial email for your sendout you can write a HTML morestachio template that will be used to generate the mail body for each mail.   
The Editor allows you to load some popular templates by fist selecting the template from the dropdown menu at the top and then using the button to download it.
You could also set your own set of templates by putting them in a folder relative to the application named "/Templates".

![image](https://user-images.githubusercontent.com/6794763/110018243-64024900-7d27-11eb-8ad8-74daaf02c5f9.png)

If you want to preview your template with the generated example data, you can click on "Open Preview Window". This will open a new Window that you can put side by side to the edtior to get a live preview of the template. It Supports both a `Text View` and a `Html View`.

![image](https://user-images.githubusercontent.com/6794763/110018506-a2980380-7d27-11eb-8d00-a367cebc5240.png)


### Data Sendout
You can send your generated mails to ether a SMTP server or save them as a `.mime` document.   

> All direct next steps will require you to validate your settings before you can continue!   
 
### Review and Sendout
When everything is entered, you can review your settings and choose to use a parallel sendout. When using the parallel method you can set the amount of threads and buffered mails in memory.

![image](https://user-images.githubusercontent.com/6794763/110018561-b3487980-7d27-11eb-9506-b0b5e52d84f1.png)

When finished, you can ether rest the UI to repeat the operation or generate a "Send Report" containing all failed to send mails.

![image](https://user-images.githubusercontent.com/6794763/110018626-c22f2c00-7d27-11eb-9ce7-717894a1b411.png)

