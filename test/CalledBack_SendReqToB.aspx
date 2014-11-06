<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CalledBack_SendReqToB.aspx.cs" Inherits="test.CalledBack_SendReqToB"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Test for Tweet compatibility</title>
    <link rel="Stylesheet" href="~/App_Themes/Site.css" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">

        <div id="userA" runat="server" style="padding-left:5em;padding-top:5em;padding-bottom:5em">
            Enter Twitter User ID you would like to Request comparison of followers and friends:<br />
            &nbsp;<asp:TextBox ID="tbxUserA" runat="server">
            </asp:TextBox>
            <asp:Button ID="btnSend" runat="server" Text="Send" OnClick="btnSend_Click" />
            <p>This compatibility Checkup does the following:</p>
            <ol>
                <li>Read your Followers and Following information</li>
                <li>Send Message with URL to your friend (One of your follower)</li>
                <li>If your friend goes to the URL, it will read your friend's Follower and Following information</li>
                <li>Compare You and your friend's following and follower information and return the comparison result</li>
            </ol>

            <div id="divMessage" runat="server"></div>
        </div>
        <div id="userB" runat="server" style="align-self: center">
            <label id="lblrequester" runat="server" />
            <br />
            <hr />
            Follower: 
            <label id="lblflstat" runat="server" />
            <br />
            <asp:DataList ID="dlFollower" runat="server" CellPadding="2">
                <ItemTemplate>
                    <asp:Label ID="mylbl" runat="server" Text='<%# Container.DataItem.ToString() %>' />
                </ItemTemplate>
            </asp:DataList>
            <hr />
            Following:
            <label id="lblfrstat" runat="server" />
            <br />
            <asp:DataList ID="dlFriend" runat="server" CellPadding="2">
                <ItemTemplate>
                    <asp:Label ID="mylblFriend" runat="server" Text='<%# Container.DataItem.ToString() %>'/>
                </ItemTemplate>
            </asp:DataList>
        </div>
        <div id="divError" runat="server" style="color:red"></div>
    </form>
</body>
</html>
