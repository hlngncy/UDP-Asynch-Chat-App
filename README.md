# Asynchronous Chat Application
This is an example of chat application which includes most of requirements that needed for server and clients. Created with .NET UDP sockets for broadcasting. 
 With this app you are able to broadcast 64kb data. You can change it easily on source code on chatserver.cs . Also does not have any database access
so does not contains any data permamently. I hope it can help you and can fulfill your purpose. 


# Features
✔ Can broadcast text and image.<br>
✔ Using Json for data transfer.<br>
✔ Has forms.<br>
✔ Servers can contain the clients for sending data to specific server. For using it broadcast the "\<DISCOVER>" text.<br>
 







## Changing Data Capasity
Dont needed the change from client side. Its optimized on your data which you want to send.
 But on the server side you need to.
 You can change the values from that line:
```c#
//chatserver.cs line 36
saea.SetBuffer(new byte[64000], 0, 64000);

```




## Executing
You can find executable files in bin files. Also you can execute client and server app as form or console.

## Note that
I know its a little messy but still on progress, i will continue to work on it. 
Also this is a project from learning state.

# Let me Know 
If you have any problems, questions or advice just let me know. 


