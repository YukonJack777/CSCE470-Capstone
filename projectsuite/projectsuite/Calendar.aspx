<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Main.master" CodeBehind="Calendar.aspx.cs" Inherits="projectsuite._Default" %>


<%@ Register assembly="DevExpress.Web.ASPxScheduler.v14.2, Version=14.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web.ASPxScheduler" tagprefix="dxwschs" %>
<%@ Register assembly="DevExpress.XtraScheduler.v14.2.Core, Version=14.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.XtraScheduler" tagprefix="cc1" %>

<asp:Content ID="Content" ContentPlaceHolderID="MainContent" runat="server">
    <table class="nav-justified">
        <tr>
            <td style="vertical-align: top">
                <dxwschs:ASPxScheduler ID="ASPxScheduler1" runat="server" ActiveViewType="Month">
                    <Views>
                        <WeekView Enabled="false">
                        </WeekView>
                        <FullWeekView Enabled="true">
                        </FullWeekView>
                    </Views>
                </dxwschs:ASPxScheduler>
            </td>
            <td rowspan="1" style="vertical-align: top">
                <dxwschs:ASPxDateNavigator ID="ASPxDateNavigator1" runat="server" ClientIDMode="AutoID" MasterControlID="ASPxScheduler1">
                    <Properties Rows="3">
                    </Properties>
                </dxwschs:ASPxDateNavigator>
            </td>
        </tr>
    </table>
    
</asp:Content>
