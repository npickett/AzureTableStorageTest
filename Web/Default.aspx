<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Web.Default"
    EnableViewState="true" Async="true" AsyncTimeout="20" %>

<%@ Register Assembly="System.Web.Entity, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
    Namespace="System.Web.UI.WebControls" TagPrefix="asp" %>
<%@ Import Namespace="Data" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Azure Storage Table Testing</title>
</head>
<body>
    <form id="statusForm" runat="server">
    <h2>Azure Storage Table Testing</h2>
    <div>
        <asp:Label ID="Label2" runat="server" Text="# of Enteries"></asp:Label>
        &nbsp;&nbsp;
        <asp:TextBox ID="txtEnteries" runat="server"></asp:TextBox>
    </div>
    <div>
        <asp:Label ID="Label3" runat="server" Text="Entry Size (bytes)"></asp:Label>
        &nbsp;&nbsp;
        <asp:TextBox ID="txtSize" runat="server"></asp:TextBox>
    </div>


    <p class="centered">
        <asp:Button ID="btnRun" runat="server" Text="Run Test" OnClick="btnRun_Click"
            EnableViewState="False" />
    </p>
    <p class="centered">
        <asp:Button ID="btnRecreate" runat="server" Text="Recreate Table" OnClick="btnRecreate_Click"
            EnableViewState="False" />
    </p>
    <p class="centered">
        <asp:Button ID="btnClearSession" runat="server" Text="Clear Results Data" OnClick="btnClearSession_Click"
            EnableViewState="False" />
    </p>
    <div class="progress_table">
        <asp:Literal ID="litNoProgress" runat="server" EnableViewState="False" Text="No progress reported yet."></asp:Literal>
        <asp:Repeater ID="InProgress" runat="server" EnableViewState="False">
            <HeaderTemplate>
                <table>
                    <thead>
                        <tr>
                            <td>
                                Operation Type
                            </td>
                            <td>
                                Batch Count
                            </td>
                            <td>
                                Start Date
                            </td>
                            <td>
                                End Date
                            </td>
                            <td>
                                ElapsedTime (Milliseconds)
                            </td>
                            <td>
                                Is Complete?
                            </td>
                            <td>
                                Is Success?
                            </td>
                            <td>
                                Is Batch?
                            </td>
                            <td>
                                Partition Key
                            </td>
                            <td>
                                Error Code
                            </td>
                            <td>
                                Status Code
                            </td>
                        </tr>
                    </thead>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <%#Eval("OperationType")%>
                    </td>
                    <td>
                        <%#Eval("BatchCount")%>
                    </td>
                    <td>
                        <%#Eval("StartDate")%>
                    </td>
                    <td>
                        <%#Eval("EndDate")%>
                    </td>
                    <td>
                        <%#Eval("ElapsedTime")%>
                    </td>
                    <td>
                        <%#Eval("IsComplete")%>
                    </td>
                    <td>
                        <%#Eval("IsSuccess")%>
                    </td>
                    <td>
                        <%#Eval("IsBatch")%>
                    </td>
                    <td>
                        <%#Eval("PartitionKey")%>
                    </td>
                    <td>
                        <%#Eval("ErrorCode")%>
                    </td>
                    <td>
                        <%#Eval("StatusCode")%>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>
    </form>
</body>
</html>
