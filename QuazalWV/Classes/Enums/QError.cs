namespace QuazalWV
{
    /// <summary>
    /// Quazal OSDK error names and codes found in the ACB binary's 'Quazal::BackEndServices::FormatQErrorCodeString' method.
    /// <seealso cref="https://github.com/kinnay/NintendoClients/blob/master/nintendo/nex/errors.py"/>
    /// </summary>
  /* 0x8064000b
        
        0x8006000c
        0x8064000a
        0x80030001
        0x80010001
        0x80050003
        0x80050001 -> 0x8005000F //UPNP THREAD
        0x80050008
        0x80030065
        0x8003006a

        0x800A0001-0x800A000B//POPULATECONFIG ERROR CODES


        0x80640001 - 0x8064000e//JOIN SESSION
        0x60002
        0x60001
        0x80060001

        //Core_Unknown =                                      0x00010001,*/
    public enum QError : uint
    {
        UNK_1 = 0x80010002,
        UNK_2 = 0x8001000d,
        

        Core_OperationAborted =                             0x80010004,
        Core_Exception =                                    0x80010005,
        Core_AccessDenied =                                 0x80010006,
        Core_InvalidArgument =                              0x8001000A,
        Core_Timeout =                                      0x8001000B,
        Core_InitializationFailure =                        0x8001000C,
        
        RendezVous_ConnectionFailure =                      0x80030001,
        RendezVous_NotAuthenticated =                       0x80030002,
        RendezVous_InvalidUsername =                        0x80030064,
        RendezVous_InvalidPassword =                        0x80030065,
        RendezVous_UsernameAlreadyExists =                  0x80030066,
        RendezVous_AccountDisabled =                        0x80030067,
        RendezVous_AccountExpired =                         0x80030068,
        RendezVous_ConcurrentLoginDenied =                  0x80030069,
        RendezVous_EncryptionFailure =                      0x8003006A,
        RendezVous_InvalidPID =                             0x8003006B,
        RendezVous_InvalidOperationInLiveEnvironment =      0x8003006F,

        PythonCore_Exception =                              0x80040001,
        PythonCore_TypeError =                              0x80040002,
        PythonCore_IndexError =                             0x80040003,
        PythonCore_InvalidReference =                       0x80040004,
        PythonCore_CallFailure =                            0x80040005,
        PythonCore_MemoryError =                            0x80040006,
        PythonCore_KeyError =                               0x80040007,
        PythonCore_OperationError =                         0x80040008,
        PythonCore_ConversionError =                        0x80040009,
        PythonCore_ValidationError =                        0x8004000A,

        Transport_ConnectionFailure =                       0x80050002,

        UbiAuthentication_Unknown =                         0x80090001,
        UbiAuthentication_Generic =                         0x80090002,
        UbiAuthentication_MissingParameter =                0x80090003,
        UbiAuthentication_UbiAccountNotFound =              0x80090004,
        UbiAuthentication_AccessDenied =                    0x80090006,
        UbiAuthentication_ExternalAccountAlreadyLinked =    0x80090007,
        UbiAuthentication_InvalidParameter =                0x80090008,

        Privilege_Unknown =                                 0x800B0001,
        Privilege_CommunicationFailure =                    0x800B0002,
        Privilege_KeyActivationNeeded =                     0x800B0003,
        Privilege_InvalidKey =                              0x800B0004,
        Privilege_KeyAlreadyActivated =                     0x800B0005,
        Privilege_KeyAlreadyActivatedByAnotherUser =        0x800B0006,
        Privilege_ExpectedPrivilegeConditionsNotMet =       0x800B0007,
        Privilege_PrivilegeNotFound =                       0x800B03E9,
        Privilege_UserNotFound =                            0x800B03EA,
        
        Tracking_Unknown =                                  0x800D0001,
        Tracking_Stop =                                     0x800D0002,

        News_Unknown =                                      0x800F0001,
        News_ChannelNotAvailable =                          0x800F0002,
        News_DuplicatedChannels =                           0x800F0003,

        Friends_Unknown =                                   0x80100001,
        Friends_MaximumFriendsLimitReached =                0x80100002,
        Friends_RecipientMaximumFriendsLimitReached =       0x80100003,
        Friends_UserNotFound =                              0x80100004,
        Friends_CannotAddBlacklistedPlayer =                0x80100005,
        Friends_CannotAddYourselfAsFriend =                 0x80100006,

        GameSession_Unknown =                               0x80120001,
        GameSession_InvalidArgument =                       0x80120002,
        GameSession_InvalidSessionKey =                     0x80120003,
        GameSession_InvalidQueryID =                        0x80120004,
        GameSession_HostPrivilegeRequired =                 0x80120005,
        GameSession_NoPublicSlotLeft =                      0x80120006,
        GameSession_NoPrivateSlotLeft =                     0x80120007,
        GameSession_HostOrParticipantRequired =             0x80120008,
        GameSession_ParticipantIsNotConnected =             0x80120009,
        GameSession_PlayerIsNotSessionParticipant =         0x8012000A,
        GameSession_PlayerIsAlreadySessionParticipant =     0x8012000B,
        GameSession_InvalidPID =                            0x8012000C,
        GameSession_GameSessionGeneratedPyFileNotFound =    0x8012000D,
    }
}
