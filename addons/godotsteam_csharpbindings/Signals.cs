using Godot;

namespace GodotSteam;

public static class Signals
{
    public static readonly StringName SteamworksError = "steamworks_error";
    public static readonly StringName FileDetailsResult = "file_details_result";
    public static readonly StringName DlcInstalled = "dlc_installed";
    public static readonly StringName NewLaunchUrlParameters = "new_launch_url_parameters";
    public static readonly StringName TimedTrialStatus = "timed_trial_status";
    public static readonly StringName AppInstalled = "app_installed";
    public static readonly StringName AppUninstalled = "app_uninstalled";
    public static readonly StringName AvatarLoaded = "avatar_loaded";
    public static readonly StringName AvatarImageLoaded = "avatar_image_loaded";
    public static readonly StringName RequestClanOfficerListSignal = "request_clan_officer_list";
    public static readonly StringName ClanActivityDownloaded = "clan_activity_downloaded";
    public static readonly StringName FriendRichPresenceUpdate = "friend_rich_presence_update";
    public static readonly StringName EnumerateFollowingListSignal = "enumerate_following_list";
    public static readonly StringName GetFollowerCountSignal = "get_follower_count";
    public static readonly StringName IsFollowingSignal = "is_following";
    public static readonly StringName ConnectedChatJoin = "connected_chat_join";
    public static readonly StringName ConnectedChatLeave = "connected_chat_leave";
    public static readonly StringName ConnectedClanChatMessage = "connected_clan_chat_message";
    public static readonly StringName ConnectedFriendChatMessage = "connected_friend_chat_message";
    public static readonly StringName JoinRequested = "join_requested";
    public static readonly StringName OverlayToggled = "overlay_toggled";
    public static readonly StringName JoinGameRequested = "join_game_requested";
    public static readonly StringName ChangeServerRequested = "change_server_requested";
    public static readonly StringName JoinClanChatComplete = "join_clan_chat_complete";
    public static readonly StringName PersonaStateChange = "persona_state_change";
    public static readonly StringName NameChanged = "name_changed";
    public static readonly StringName OverlayBrowserProtocol = "overlay_browser_protocol";
    public static readonly StringName UnreadChatMessagesChanged = "unread_chat_messages_changed";
    public static readonly StringName EquippedProfileItemsChanged =
        "equipped_profile_items_changed";
    public static readonly StringName EquippedProfileItems = "equipped_profile_items";
    public static readonly StringName SearchForGameProgress = "search_for_game_progress";
    public static readonly StringName SearchForGameResult = "search_for_game_result";
    public static readonly StringName RequestPlayersForGameProgress =
        "request_players_for_game_progress";
    public static readonly StringName RequestPlayersForGameResult =
        "request_players_for_game_result";
    public static readonly StringName RequestPlayersForGameFinalResult =
        "request_players_for_game_final_result";
    public static readonly StringName SubmitPlayerResultSignal = "submit_player_result";
    public static readonly StringName EndGameResult = "end_game_result";
    public static readonly StringName HtmlBrowserReady = "html_browser_ready";
    public static readonly StringName HtmlCanGoBackandforward = "html_can_go_backandforward";
    public static readonly StringName HtmlChangedTitle = "html_changed_title";
    public static readonly StringName HtmlCloseBrowser = "html_close_browser";
    public static readonly StringName HtmlFileOpenDialog = "html_file_open_dialog";
    public static readonly StringName HtmlFinishedRequest = "html_finished_request";
    public static readonly StringName HtmlHideTooltip = "html_hide_tooltip";
    public static readonly StringName HtmlHorizontalScroll = "html_horizontal_scroll";
    public static readonly StringName HtmlJsAlert = "html_js_alert";
    public static readonly StringName HtmlJsConfirm = "html_js_confirm";
    public static readonly StringName HtmlLinkAtPosition = "html_link_at_position";
    public static readonly StringName HtmlNeedsPaint = "html_needs_paint";
    public static readonly StringName HtmlNewWindow = "html_new_window";
    public static readonly StringName HtmlOpenLinkInNewTab = "html_open_link_in_new_tab";
    public static readonly StringName HtmlSearchResults = "html_search_results";
    public static readonly StringName HtmlSetCursor = "html_set_cursor";
    public static readonly StringName HtmlShowTooltip = "html_show_tooltip";
    public static readonly StringName HtmlStartRequest = "html_start_request";
    public static readonly StringName HtmlStatusText = "html_status_text";
    public static readonly StringName HtmlUpdateTooltip = "html_update_tooltip";
    public static readonly StringName HtmlUrlChanged = "html_url_changed";
    public static readonly StringName HtmlVerticalScroll = "html_vertical_scroll";
    public static readonly StringName HttpRequestCompleted = "http_request_completed";
    public static readonly StringName HttpRequestDataReceived = "http_request_data_received";
    public static readonly StringName HttpRequestHeadersReceived = "http_request_headers_received";
    public static readonly StringName InputActionEvent = "input_action_event";
    public static readonly StringName InputDeviceConnected = "input_device_connected";
    public static readonly StringName InputDeviceDisconnected = "input_device_disconnected";
    public static readonly StringName InputConfigurationLoaded = "input_configuration_loaded";
    public static readonly StringName InputGamepadSlotChange = "input_gamepad_slot_change";
    public static readonly StringName InventoryDefinitionUpdate = "inventory_definition_update";
    public static readonly StringName InventoryEligiblePromoItem = "inventory_eligible_promo_item";
    public static readonly StringName InventoryFullUpdate = "inventory_full_update";
    public static readonly StringName InventoryResultReady = "inventory_result_ready";
    public static readonly StringName InventoryStartPurchaseResult =
        "inventory_start_purchase_result";
    public static readonly StringName InventoryRequestPricesResult =
        "inventory_request_prices_result";
    public static readonly StringName FavoritesListAccountsUpdated =
        "favorites_list_accounts_updated";
    public static readonly StringName FavoritesListChanged = "favorites_list_changed";
    public static readonly StringName LobbyMessage = "lobby_message";
    public static readonly StringName LobbyChatUpdate = "lobby_chat_update";
    public static readonly StringName LobbyCreated = "lobby_created";
    public static readonly StringName LobbyDataUpdate = "lobby_data_update";
    public static readonly StringName LobbyJoined = "lobby_joined";
    public static readonly StringName LobbyGameCreated = "lobby_game_created";
    public static readonly StringName LobbyInvite = "lobby_invite";
    public static readonly StringName LobbyMatchList = "lobby_match_list";
    public static readonly StringName LobbyKicked = "lobby_kicked";
    public static readonly StringName ServerResponded = "server_responded";
    public static readonly StringName ServerFailedToRespond = "server_failed_to_respond";
    public static readonly StringName MusicPlayerRemoteToFront = "music_player_remote_to_front";
    public static readonly StringName MusicPlayerRemoteWillActivate =
        "music_player_remote_will_activate";
    public static readonly StringName MusicPlayerRemoteWillDeactivate =
        "music_player_remote_will_deactivate";
    public static readonly StringName MusicPlayerSelectsPlaylistEntry =
        "music_player_selects_playlist_entry";
    public static readonly StringName MusicPlayerSelectsQueueEntry =
        "music_player_selects_queue_entry";
    public static readonly StringName MusicPlayerWantsLooped = "music_player_wants_looped";
    public static readonly StringName MusicPlayerWantsPause = "music_player_wants_pause";
    public static readonly StringName MusicPlayerWantsPlayingRepeatStatus =
        "music_player_wants_playing_repeat_status";
    public static readonly StringName MusicPlayerWantsPlayNext = "music_player_wants_play_next";
    public static readonly StringName MusicPlayerWantsPlayPrevious =
        "music_player_wants_play_previous";
    public static readonly StringName MusicPlayerWantsPlay = "music_player_wants_play";
    public static readonly StringName MusicPlayerWantsShuffled = "music_player_wants_shuffled";
    public static readonly StringName MusicPlayerWantsVolume = "music_player_wants_volume";
    public static readonly StringName MusicPlayerWillQuit = "music_player_will_quit";
    public static readonly StringName P2PSessionRequest = "p2p_session_request";
    public static readonly StringName P2PSessionConnectFail = "p2p_session_connect_fail";
    public static readonly StringName NetworkMessagesSessionRequest =
        "network_messages_session_request";
    public static readonly StringName NetworkMessagesSessionFailed =
        "network_messages_session_failed";
    public static readonly StringName NetworkConnectionStatusChanged =
        "network_connection_status_changed";
    public static readonly StringName NetworkAuthenticationStatus = "network_authentication_status";
    public static readonly StringName FakeIPResult = "fake_ip_result";
    public static readonly StringName RelayNetworkStatus = "relay_network_status";
    public static readonly StringName ParentalSettingChanged = "parental_setting_changed";
    public static readonly StringName JoinPartySignal = "join_party";
    public static readonly StringName CreateBeaconSignal = "create_beacon";
    public static readonly StringName ReservationNotification = "reservation_notification";
    public static readonly StringName ChangeNumOpenSlotsSignal = "change_num_open_slots";
    public static readonly StringName AvailableBeaconLocationsUpdated =
        "available_beacon_locations_updated";
    public static readonly StringName ActiveBeaconsUpdated = "active_beacons_updated";
    public static readonly StringName RemotePlaySessionConnected = "remote_play_session_connected";
    public static readonly StringName RemotePlaySessionDisconnected =
        "remote_play_session_disconnected";
    public static readonly StringName FileReadAsyncComplete = "file_read_async_complete";
    public static readonly StringName FileShareResult = "file_share_result";
    public static readonly StringName FileWriteAsyncComplete = "file_write_async_complete";
    public static readonly StringName DownloadUgcResult = "download_ugc_result";
    public static readonly StringName UnsubscribeItemSignal = "unsubscribe_item";
    public static readonly StringName SubscribeItemSignal = "subscribe_item";
    public static readonly StringName LocalFileChanged = "local_file_changed";
    public static readonly StringName ScreenshotReady = "screenshot_ready";
    public static readonly StringName ScreenshotRequested = "screenshot_requested";
    public static readonly StringName AddAppDependencyResult = "add_app_dependency_result";
    public static readonly StringName AddUgcDependencyResult = "add_ugc_dependency_result";
    public static readonly StringName ItemCreated = "item_created";
    public static readonly StringName ItemDownloaded = "item_downloaded";
    public static readonly StringName GetAppDependenciesResult = "get_app_dependencies_result";
    public static readonly StringName ItemDeleted = "item_deleted";
    public static readonly StringName GetItemVoteResult = "get_item_vote_result";
    public static readonly StringName ItemInstalled = "item_installed";
    public static readonly StringName RemoveAppDependencyResult = "remove_app_dependency_result";
    public static readonly StringName RemoveUgcDependencyResult = "remove_ugc_dependency_result";
    public static readonly StringName SetUserItemVoteSignal = "set_user_item_vote";
    public static readonly StringName StartPlaytimeTrackingSignal = "start_playtime_tracking";
    public static readonly StringName UgcQueryCompleted = "ugc_query_completed";
    public static readonly StringName StopPlaytimeTrackingSignal = "stop_playtime_tracking";
    public static readonly StringName ItemUpdated = "item_updated";
    public static readonly StringName UserFavoriteItemsListChanged =
        "user_favorite_items_list_changed";
    public static readonly StringName WorkshopEulaStatus = "workshop_eula_status";
    public static readonly StringName UserSubscribedItemsListChanged =
        "user_subscribed_items_list_changed";
    public static readonly StringName ClientGameServerDeny = "client_game_server_deny";
    public static readonly StringName DurationControl = "duration_control";
    public static readonly StringName EncryptedAppTicketResponse = "encrypted_app_ticket_response";
    public static readonly StringName GameWebCallback = "game_web_callback";
    public static readonly StringName GetAuthSessionTicketResponse =
        "get_auth_session_ticket_response";
    public static readonly StringName GetTicketForWebApi = "get_ticket_for_web_api";
    public static readonly StringName IpcFailure = "ipc_failure";
    public static readonly StringName LicensesUpdated = "licenses_updated";
    public static readonly StringName MicrotransactionAuthResponse =
        "microtransaction_auth_response";
    public static readonly StringName SteamServerConnectFailed = "steam_server_connect_failed";
    public static readonly StringName SteamServerConnected = "steam_server_connected";
    public static readonly StringName SteamServerDisconnected = "steam_server_disconnected";
    public static readonly StringName StoreAuthUrlResponse = "store_auth_url_response";
    public static readonly StringName ValidateAuthTicketResponse = "validate_auth_ticket_response";
    public static readonly StringName GlobalAchievementPercentagesReady =
        "global_achievement_percentages_ready";
    public static readonly StringName GlobalStatsReceived = "global_stats_received";
    public static readonly StringName LeaderboardFindResult = "leaderboard_find_result";
    public static readonly StringName LeaderboardScoresDownloaded = "leaderboard_scores_downloaded";
    public static readonly StringName LeaderboardScoreUploaded = "leaderboard_score_uploaded";
    public static readonly StringName LeaderboardUgcSet = "leaderboard_ugc_set";
    public static readonly StringName NumberOfCurrentPlayers = "number_of_current_players";
    public static readonly StringName UserAchievementStored = "user_achievement_stored";
    public static readonly StringName CurrentStatsReceived = "current_stats_received";
    public static readonly StringName UserStatsReceived = "user_stats_received";
    public static readonly StringName UserStatsStored = "user_stats_stored";
    public static readonly StringName UserStatsUnloaded = "user_stats_unloaded";
    public static readonly StringName CheckFileSignature = "check_file_signature";
    public static readonly StringName GamepadTextInputDismissed = "gamepad_text_input_dismissed";
    public static readonly StringName IPCountry = "ip_country";
    public static readonly StringName LowPower = "low_power";
    public static readonly StringName SteamApiCallCompleted = "steam_api_call_completed";
    public static readonly StringName SteamShutdownSignal = "steam_shutdown";
    public static readonly StringName AppResumingFromSuspend = "app_resuming_from_suspend";
    public static readonly StringName FloatingGamepadTextInputDismissed =
        "floating_gamepad_text_input_dismissed";
    public static readonly StringName FilterTextDictionaryChanged =
        "filter_text_dictionary_changed";
    public static readonly StringName GetOpfSettingsResult = "get_opf_settings_result";
    public static readonly StringName GetVideoResult = "get_video_result";
}
