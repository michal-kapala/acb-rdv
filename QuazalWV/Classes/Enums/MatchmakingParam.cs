namespace QuazalWV
{
	/// <summary>
	/// Property IDs used as game session attributes.
	/// </summary>
	public enum MatchmakingParam
	{
		SearchStrategy = 0,
		MaxPublicSlots = 3,
		MaxPrivateSlots = 4,
		CurrentPublicSlots = 5,
		CurrentPrivateSlots = 6,
		Unk7 = 7,
		FreePublicSlots = 0x32,
		FreePrivateSlots = 0x33,
		CxbCrcSum = 0x64,
		MapID = 0x65,
		Unk66 = 0x66,
		Unk67 = 0x67,
		Unk68 = 0x68,
		MinLevelIncrease = 0x69,
		MaxLevelIncrease = 0x6A,
		Unk6B = 0x6B,
		GameMode = 0x6C,
		QueryMaxSlotsTaken = 0x6D,
		GameType = 0x6E,
		Accessibility = 0x6F,
		DlcValue = 0x70,
		Unk71 = 0x71,
		NatType = 0x72,
		PunkbusterActive = 0x73
	}
}
