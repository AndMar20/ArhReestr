namespace WebApp.ViewModels;

public record ChatDialogItem(int RealEstateId, int PeerId, string PeerName, string RealEstateAddress, string LastMessage, DateTime LastMessageAt, bool HasUnread);
