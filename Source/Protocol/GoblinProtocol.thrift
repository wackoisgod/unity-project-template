namespace csharp LibCommon.Network.Types

typedef string Timestamp

enum MessageType
{
	TestEvent,
}

struct GameMessage
{
	1: required MessageType eventType,
	2: required string content,
	3: required string networkId,
}

struct GameMessageList
{
	1: required list<string> messages,
}

struct TestEvent
{
	
}