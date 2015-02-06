<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SearchEngineDemo.aspx.cs" Inherits="SearchEngineDemo" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:Button ID="Button1" runat="server" Text="Searching" 
            onclick="Button1_Click" />
        <br />
        <br />
        <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
        <br />
        
        <p>
        <asp:Label ID="Label1" runat="server" Text="Return Value"></asp:Label>
    
        </p>
        <asp:Label ID="TextBox1" runat="server" Height="334px" Width="891px"></asp:Label>
    
    </div>
    </form>
</body>
</html>
