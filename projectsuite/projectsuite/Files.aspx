<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Main.master" CodeBehind="Files.aspx.cs" Inherits="projectsuite._Default" %>


<asp:Content ID="Content" ContentPlaceHolderID="MainContent" runat="server">
    <dx:ASPxFileManager ID="ASPxFileManager1" runat="server" Height="720px">
        <Settings RootFolder="~/Content/FileManager/Files"
            ThumbnailFolder="~/Content/FileManager/Thumbnails"
            InitialFolder=""/>
        <SettingsEditing AllowCopy="True" AllowCreate="True" AllowDelete="True" AllowDownload="True" AllowMove="True" AllowRename="True" />
        <SettingsUpload>
            <AdvancedModeSettings EnableMultiSelect="True">
            </AdvancedModeSettings>
        </SettingsUpload>
    </dx:ASPxFileManager>
</asp:Content>
