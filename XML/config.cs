﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class configuration {
    
    private string wSUsernameField;
    
    private string wSPasswordField;
    
    private bool servicesEnabledField;
    
    private int serverPortField;
    
    private string motion_WSActivity_iSpyField;
    
    private bool enable_Error_ReportingField;
    
    private bool enable_Update_CheckField;
    
    private string password_Protect_PasswordField;
    
    private bool enable_Password_ProtectField;
    
    private string mJPEGURLField;
    
    private string aVIFileNameField;
    
    private string recentJPGListField;
    
    private string recentMJPGListField;
    
    private string recentFileListField;
    
    private int nextCameraIDField;
    
    private int nextMicrophoneIDField;
    
    private bool subscribedField;
    
    private string jPEGURLField;
    
    private string activityColorField;
    
    private string noActivityColorField;
    
    private string trackingColorField;
    
    private string volumeLevelColorField;
    
    private string mainColorField;
    
    private string areaColorField;
    
    private string backColorField;
    
    private int maxMediaFolderSizeMBField;
    
    private int deleteFilesOlderThanDaysField;
    
    private string serverAddressField;
    
    private bool enabled_ShowGettingStartedField;
    
    private int opacityField;
    
    private int lANPortField;
    
    private bool enable_Storage_ManagementField;
    
    private string fFMPEG_CameraField;
    
    private string fFMPEG_MicrophoneField;
    
    private string timestampColorField;
    
    private bool useUPNPField;
    
    private int nextFloorPlanIDField;
    
    private string youTubeUsernameField;
    
    private string youTubePasswordField;
    
    private string youTubeAccountField;
    
    private string youTubeKeyField;
    
    private string youTubeCategoriesField;
    
    private string youTubeDefaultCategoryField;
    
    private string compressorOptionsField;
    
    private int nextCommandIDField;
    
    private bool logFFMPEGCommandsField;
    
    private bool balloonTipsField;
    
    private string trayIconTextField;
    
    private bool autoLayoutField;
    
    private string upnpDeviceField;
    
    private string iPModeField;
    
    private string iPv4AddressField;
    
    private string iPv6AddressField;
    
    private bool dHCPRerouteField;
    
    private string mediaDirectoryField;
    
    private bool fFMPEG_SingleProcessField;
    
    private bool loopbackField;
    
    private int serverReceiveTimeoutField;
    
    private int iPCameraTimeoutField;
    
    private string serverNameField;
    
    private string uploadedVideosField;
    
    private string languageField;
    
    private string recentVLCListField;
    
    private string vLCURLField;
    
    private bool autoScheduleField;
    
    private bool fullscreenField;
    
    private bool showStatusField;
    
    private bool showToolbarField;
    
    private bool showFileMenuField;
    
    private bool alwaysOnTopField;
    
    private int logFileSizeKBField;

    private int CPUMaxField;

    private string AllowedIPListField;

    private int MaxRecordingThreadsField;

    /// <remarks/>
    public int MaxRecordingThreads
    {
        get
        {
            return this.MaxRecordingThreadsField;
        }
        set
        {
            this.MaxRecordingThreadsField = value;
        }
    }

     /// <remarks/>
    public string AllowedIPList
    {
        get {
            return this.AllowedIPListField;
        }
        set {
            this.AllowedIPListField = value;
        }
    }

    /// <remarks/>
    public int CPUMax
    {
        get
        {
            return this.CPUMaxField;
        }
        set
        {
            this.CPUMaxField = value;
        }
    }

    /// <remarks/>
    public string WSUsername {
        get {
            return this.wSUsernameField;
        }
        set {
            this.wSUsernameField = value;
        }
    }
    
    /// <remarks/>
    public string WSPassword {
        get {
            return this.wSPasswordField;
        }
        set {
            this.wSPasswordField = value;
        }
    }
    
    /// <remarks/>
    public bool ServicesEnabled {
        get {
            return this.servicesEnabledField;
        }
        set {
            this.servicesEnabledField = value;
        }
    }
    
    /// <remarks/>
    public int ServerPort {
        get {
            return this.serverPortField;
        }
        set {
            this.serverPortField = value;
        }
    }
    
    /// <remarks/>
    public string motion_WSActivity_iSpy {
        get {
            return this.motion_WSActivity_iSpyField;
        }
        set {
            this.motion_WSActivity_iSpyField = value;
        }
    }
    
    /// <remarks/>
    public bool Enable_Error_Reporting {
        get {
            return this.enable_Error_ReportingField;
        }
        set {
            this.enable_Error_ReportingField = value;
        }
    }
    
    /// <remarks/>
    public bool Enable_Update_Check {
        get {
            return this.enable_Update_CheckField;
        }
        set {
            this.enable_Update_CheckField = value;
        }
    }
    
    /// <remarks/>
    public string Password_Protect_Password {
        get {
            return this.password_Protect_PasswordField;
        }
        set {
            this.password_Protect_PasswordField = value;
        }
    }
    
    /// <remarks/>
    public bool Enable_Password_Protect {
        get {
            return this.enable_Password_ProtectField;
        }
        set {
            this.enable_Password_ProtectField = value;
        }
    }
    
    /// <remarks/>
    public string MJPEGURL {
        get {
            return this.mJPEGURLField;
        }
        set {
            this.mJPEGURLField = value;
        }
    }
    
    /// <remarks/>
    public string AVIFileName {
        get {
            return this.aVIFileNameField;
        }
        set {
            this.aVIFileNameField = value;
        }
    }
    
    /// <remarks/>
    public string RecentJPGList {
        get {
            return this.recentJPGListField;
        }
        set {
            this.recentJPGListField = value;
        }
    }
    
    /// <remarks/>
    public string RecentMJPGList {
        get {
            return this.recentMJPGListField;
        }
        set {
            this.recentMJPGListField = value;
        }
    }
    
    /// <remarks/>
    public string RecentFileList {
        get {
            return this.recentFileListField;
        }
        set {
            this.recentFileListField = value;
        }
    }
    
    /// <remarks/>
    public int NextCameraID {
        get {
            return this.nextCameraIDField;
        }
        set {
            this.nextCameraIDField = value;
        }
    }
    
    /// <remarks/>
    public int NextMicrophoneID {
        get {
            return this.nextMicrophoneIDField;
        }
        set {
            this.nextMicrophoneIDField = value;
        }
    }
    
    /// <remarks/>
    public bool Subscribed {
        get {
            return this.subscribedField;
        }
        set {
            this.subscribedField = value;
        }
    }
    
    /// <remarks/>
    public string JPEGURL {
        get {
            return this.jPEGURLField;
        }
        set {
            this.jPEGURLField = value;
        }
    }
    
    /// <remarks/>
    public string ActivityColor {
        get {
            return this.activityColorField;
        }
        set {
            this.activityColorField = value;
        }
    }
    
    /// <remarks/>
    public string NoActivityColor {
        get {
            return this.noActivityColorField;
        }
        set {
            this.noActivityColorField = value;
        }
    }
    
    /// <remarks/>
    public string TrackingColor {
        get {
            return this.trackingColorField;
        }
        set {
            this.trackingColorField = value;
        }
    }
    
    /// <remarks/>
    public string VolumeLevelColor {
        get {
            return this.volumeLevelColorField;
        }
        set {
            this.volumeLevelColorField = value;
        }
    }
    
    /// <remarks/>
    public string MainColor {
        get {
            return this.mainColorField;
        }
        set {
            this.mainColorField = value;
        }
    }
    
    /// <remarks/>
    public string AreaColor {
        get {
            return this.areaColorField;
        }
        set {
            this.areaColorField = value;
        }
    }
    
    /// <remarks/>
    public string BackColor {
        get {
            return this.backColorField;
        }
        set {
            this.backColorField = value;
        }
    }
    
    /// <remarks/>
    public int MaxMediaFolderSizeMB {
        get {
            return this.maxMediaFolderSizeMBField;
        }
        set {
            this.maxMediaFolderSizeMBField = value;
        }
    }
    
    /// <remarks/>
    public int DeleteFilesOlderThanDays {
        get {
            return this.deleteFilesOlderThanDaysField;
        }
        set {
            this.deleteFilesOlderThanDaysField = value;
        }
    }
    
    /// <remarks/>
    public string ServerAddress {
        get {
            return this.serverAddressField;
        }
        set {
            this.serverAddressField = value;
        }
    }
    
    /// <remarks/>
    public bool Enabled_ShowGettingStarted {
        get {
            return this.enabled_ShowGettingStartedField;
        }
        set {
            this.enabled_ShowGettingStartedField = value;
        }
    }
    
    /// <remarks/>
    public int Opacity {
        get {
            return this.opacityField;
        }
        set {
            this.opacityField = value;
        }
    }
    
    /// <remarks/>
    public int LANPort {
        get {
            return this.lANPortField;
        }
        set {
            this.lANPortField = value;
        }
    }
    
    /// <remarks/>
    public bool Enable_Storage_Management {
        get {
            return this.enable_Storage_ManagementField;
        }
        set {
            this.enable_Storage_ManagementField = value;
        }
    }
    
    /// <remarks/>
    public string FFMPEG_Camera {
        get {
            return this.fFMPEG_CameraField;
        }
        set {
            this.fFMPEG_CameraField = value;
        }
    }
    
    /// <remarks/>
    public string FFMPEG_Microphone {
        get {
            return this.fFMPEG_MicrophoneField;
        }
        set {
            this.fFMPEG_MicrophoneField = value;
        }
    }
    
    /// <remarks/>
    public string TimestampColor {
        get {
            return this.timestampColorField;
        }
        set {
            this.timestampColorField = value;
        }
    }
    
    /// <remarks/>
    public bool UseUPNP {
        get {
            return this.useUPNPField;
        }
        set {
            this.useUPNPField = value;
        }
    }
    
    /// <remarks/>
    public int NextFloorPlanID {
        get {
            return this.nextFloorPlanIDField;
        }
        set {
            this.nextFloorPlanIDField = value;
        }
    }
    
    /// <remarks/>
    public string YouTubeUsername {
        get {
            return this.youTubeUsernameField;
        }
        set {
            this.youTubeUsernameField = value;
        }
    }
    
    /// <remarks/>
    public string YouTubePassword {
        get {
            return this.youTubePasswordField;
        }
        set {
            this.youTubePasswordField = value;
        }
    }
    
    /// <remarks/>
    public string YouTubeAccount {
        get {
            return this.youTubeAccountField;
        }
        set {
            this.youTubeAccountField = value;
        }
    }
    
    /// <remarks/>
    public string YouTubeKey {
        get {
            return this.youTubeKeyField;
        }
        set {
            this.youTubeKeyField = value;
        }
    }
    
    /// <remarks/>
    public string YouTubeCategories {
        get {
            return this.youTubeCategoriesField;
        }
        set {
            this.youTubeCategoriesField = value;
        }
    }
    
    /// <remarks/>
    public string YouTubeDefaultCategory {
        get {
            return this.youTubeDefaultCategoryField;
        }
        set {
            this.youTubeDefaultCategoryField = value;
        }
    }
    
    /// <remarks/>
    public string CompressorOptions {
        get {
            return this.compressorOptionsField;
        }
        set {
            this.compressorOptionsField = value;
        }
    }
    
    /// <remarks/>
    public int NextCommandID {
        get {
            return this.nextCommandIDField;
        }
        set {
            this.nextCommandIDField = value;
        }
    }
    
    /// <remarks/>
    public bool LogFFMPEGCommands {
        get {
            return this.logFFMPEGCommandsField;
        }
        set {
            this.logFFMPEGCommandsField = value;
        }
    }
    
    /// <remarks/>
    public bool BalloonTips {
        get {
            return this.balloonTipsField;
        }
        set {
            this.balloonTipsField = value;
        }
    }
    
    /// <remarks/>
    public string TrayIconText {
        get {
            return this.trayIconTextField;
        }
        set {
            this.trayIconTextField = value;
        }
    }
    
    /// <remarks/>
    public bool AutoLayout {
        get {
            return this.autoLayoutField;
        }
        set {
            this.autoLayoutField = value;
        }
    }
    
    /// <remarks/>
    public string UpnpDevice {
        get {
            return this.upnpDeviceField;
        }
        set {
            this.upnpDeviceField = value;
        }
    }
    
    /// <remarks/>
    public string IPMode {
        get {
            return this.iPModeField;
        }
        set {
            this.iPModeField = value;
        }
    }
    
    /// <remarks/>
    public string IPv4Address {
        get {
            return this.iPv4AddressField;
        }
        set {
            this.iPv4AddressField = value;
        }
    }
    
    /// <remarks/>
    public string IPv6Address {
        get {
            return this.iPv6AddressField;
        }
        set {
            this.iPv6AddressField = value;
        }
    }
    
    /// <remarks/>
    public bool DHCPReroute {
        get {
            return this.dHCPRerouteField;
        }
        set {
            this.dHCPRerouteField = value;
        }
    }
    
    /// <remarks/>
    public string MediaDirectory {
        get {
            return this.mediaDirectoryField;
        }
        set {
            this.mediaDirectoryField = value;
        }
    }
    
    /// <remarks/>
    public bool FFMPEG_SingleProcess {
        get {
            return this.fFMPEG_SingleProcessField;
        }
        set {
            this.fFMPEG_SingleProcessField = value;
        }
    }
    
    /// <remarks/>
    public bool Loopback {
        get {
            return this.loopbackField;
        }
        set {
            this.loopbackField = value;
        }
    }
    
    /// <remarks/>
    public int ServerReceiveTimeout {
        get {
            return this.serverReceiveTimeoutField;
        }
        set {
            this.serverReceiveTimeoutField = value;
        }
    }
    
    /// <remarks/>
    public int IPCameraTimeout {
        get {
            return this.iPCameraTimeoutField;
        }
        set {
            this.iPCameraTimeoutField = value;
        }
    }
    
    /// <remarks/>
    public string ServerName {
        get {
            return this.serverNameField;
        }
        set {
            this.serverNameField = value;
        }
    }
    
    /// <remarks/>
    public string UploadedVideos {
        get {
            return this.uploadedVideosField;
        }
        set {
            this.uploadedVideosField = value;
        }
    }
    
    /// <remarks/>
    public string Language {
        get {
            return this.languageField;
        }
        set {
            this.languageField = value;
        }
    }
    
    /// <remarks/>
    public string RecentVLCList {
        get {
            return this.recentVLCListField;
        }
        set {
            this.recentVLCListField = value;
        }
    }
    
    /// <remarks/>
    public string VLCURL {
        get {
            return this.vLCURLField;
        }
        set {
            this.vLCURLField = value;
        }
    }
    
    /// <remarks/>
    public bool AutoSchedule {
        get {
            return this.autoScheduleField;
        }
        set {
            this.autoScheduleField = value;
        }
    }
    
    /// <remarks/>
    public bool Fullscreen {
        get {
            return this.fullscreenField;
        }
        set {
            this.fullscreenField = value;
        }
    }
    
    /// <remarks/>
    public bool ShowStatus {
        get {
            return this.showStatusField;
        }
        set {
            this.showStatusField = value;
        }
    }
    
    /// <remarks/>
    public bool ShowToolbar {
        get {
            return this.showToolbarField;
        }
        set {
            this.showToolbarField = value;
        }
    }
    
    /// <remarks/>
    public bool ShowFileMenu {
        get {
            return this.showFileMenuField;
        }
        set {
            this.showFileMenuField = value;
        }
    }
    
    /// <remarks/>
    public bool AlwaysOnTop {
        get {
            return this.alwaysOnTopField;
        }
        set {
            this.alwaysOnTopField = value;
        }
    }
    
    /// <remarks/>
    public int LogFileSizeKB {
        get {
            return this.logFileSizeKBField;
        }
        set {
            this.logFileSizeKBField = value;
        }
    }
}
