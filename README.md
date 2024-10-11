## Usage
A fake SMTP server for testing.  
  
Only support AUTH LOGIN and below sequences,  
EHLO -> AUTH LOGIN -> user in BASE64 encoding -> password in BASE64 encoding -> MAIL FROM -> RCPT TO -> DATA -> QUIT  
Refer to Command.cs for more response details.  

