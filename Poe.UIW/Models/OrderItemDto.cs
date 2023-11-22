namespace Poe.UIW.Models;

public class OrderItemDto
{
    public string Id { get; set; }
    public string OrderName { get; set; }
    public long OrderId { get; set; }
    public string WhisperMessage { get; set; }
    public string WhisperToken { get; set; }
    public ItemPrice Price { get; set; }
    public string ImageUrl { get; set; }
    public bool NameExists { get; set; }
    public string Name { get; set; }
    public string TypeLine { get; set; }
    public ItemKind ItemKind { get; set; }
    public ItemInfo ItemInfo { get; set; }
}
public class NotableProperty
{
    public string Name { get; set; }

    public string[] Values { get; set; }
}

public class ItemInfo
{
    public ItemFrameType ItemFrameType { get; set; }
    public int FoilVariation { get; set; }

    public ItemInfoProperty[] Properties { get; set; }
    public ItemInfoExtendedProperty[] ExtendedProperties { get; set; }
    
    public ItemInfoRequirement[] Requirements { get; set; }

    public string[] EnchantMods { get; set; }
    public NotableProperty[] NotableProperties { get; set; }

    public string[] ImplicitMods { get; set; }
    public string[] ExplicitMods { get; set; }
    public string[] FracturedMods { get; set; }
    public string[] CraftedMods { get; set; }
    
    public LogbookMods[] LogbookMods { get; set; }

    public bool IsCorrupted { get; set; }
    public bool IsIdentified { get; set; }
    public bool IsDuplicated { get; set; }
    public bool IsSplitted { get; set; }
    public bool IsFractured { get; set; }
    public bool IsReplica { get; set; }
    public bool IsSynthetic { get; set; }
    public bool IsVeiled { get; set; }
    public bool IsSearing { get; set; }
    public bool IsTangled { get; set; }
    public bool IsLogBook { get; set; }
    public bool ExistsEnchantMods { get; set; }
    public bool ExistsNotableProperties { get; set; }
    public bool ExistsImplicitMods { get; set; }
    public bool ExistsExplicitMods { get; set; }
    public string[] Influences { get; set; }
    
    public Sockets Sockets { get; set; }

    public bool HasItemLevel { get; set; }
    public int ItemLevel { get; set; }
}

public class LogbookMods
{
    public string Name { get; set; }
    
    public string FactionName { get; set; }
    
    public string[] Mods { get; set; }
}


public class Sockets
{
    public bool IsVertical { get; set; }
    public int Count { get; set; }
    public SocketGroup[] Groups { get; set; }
}

public class SocketGroup
{
    public Socket[] Sockets { get; set; }
}

public class Socket
{
    public int OrdinalNumber { get; set; }
    public SocketColor Color { get; set; }
}

public class ItemInfoProperty
{
    public string Name { get; set; }
    public string Value { get; set; }
    public bool NonValue { get; set; }
}

public class ItemInfoExtendedProperty
{
    public string Name { get; set; }
    public string Value { get; set; }
}

public class ItemInfoRequirement
{
    public string Name { get; set; }
    public string Value { get; set; }
}