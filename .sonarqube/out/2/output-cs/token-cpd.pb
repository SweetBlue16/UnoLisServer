…
ZC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Properties\AssemblyInfo.cs
[ 
assembly 	
:	 

AssemblyTitle 
( 
$str 1
)1 2
]2 3
[		 
assembly		 	
:			 

AssemblyDescription		 
(		 
$str		 !
)		! "
]		" #
[

 
assembly

 	
:

	 
!
AssemblyConfiguration

  
(

  !
$str

! #
)

# $
]

$ %
[ 
assembly 	
:	 

AssemblyCompany 
( 
$str 
)  
]  !
[ 
assembly 	
:	 

AssemblyProduct 
( 
$str 3
)3 4
]4 5
[ 
assembly 	
:	 

AssemblyCopyright 
( 
$str 2
)2 3
]3 4
[ 
assembly 	
:	 

AssemblyTrademark 
( 
$str 
)  
]  !
[ 
assembly 	
:	 

AssemblyCulture 
( 
$str 
) 
] 
[ 
assembly 	
:	 


ComVisible 
( 
false 
) 
] 
[ 
assembly 	
:	 

Guid 
( 
$str 6
)6 7
]7 8
[   
assembly   	
:  	 

AssemblyVersion   
(   
$str   $
)  $ %
]  % &
[!! 
assembly!! 	
:!!	 

AssemblyFileVersion!! 
(!! 
$str!! (
)!!( )
]!!) *ß
ZC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IShopManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /
IShopCallback/ <
)< =
,= >
SessionMode? J
=K L
SessionModeM X
.X Y
RequiredY a
)a b
]b c
public 

	interface 
IShopManager !
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
GetShopItems 
( 
) 
; 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
PurchaseItem 
( 
PurchaseRequest )
request* 1
)1 2
;2 3
} 
[ 
ServiceContract 
] 
public 

	interface 
IShopCallback "
:# $
ISessionCallback% 5
{ 
[ 	
OperationContract	 
] 
void 
ShopItemsReceived 
( 
List #
<# $
ShopItem$ ,
>, -
items. 3
)3 4
;4 5
[ 	
OperationContract	 
] 
void 
PurchaseResponse 
( 
bool "
success# *
,* +
string, 2
itemName3 ;
); <
;< =
} 
} ö
^C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\ISessionCallback.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !

Interfaces! +
{		 
[ 
ServiceContract 
] 
public 

	interface 
ISessionCallback %
{ 
[ 	
OperationContract	 
] 
void 
SessionExpired 
( 
) 
; 
[ 	
OperationContract	 
] 
void 
PlayerDisconnected 
(  
string  &
nickname' /
)/ 0
;0 1
} 
} 
^C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IRegisterManager.cs
	namespace

 	
UnoLisServer


 
.

 
	Contracts

  
.

  !

Interfaces

! +
{ 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /
IRegisterCallback/ @
)@ A
,A B
SessionModeC N
=O P
SessionModeQ \
.\ ]
Required] e
)e f
]f g
public 

	interface 
IRegisterManager %
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
Register 
( 
RegistrationData &
data' +
)+ ,
;, -
} 
[ 
ServiceContract 
] 
public 

	interface 
IRegisterCallback &
:' (
ISessionCallback) 9
{ 
[ 	
OperationContract	 
] 
void 
RegisterResponse 
( 
ServiceResponse -
<- .
object. 4
>4 5
response6 >
)> ?
;? @
} 
} £
aC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IProfileViewManager.cs
	namespace

 	
UnoLisServer


 
.

 
	Contracts

  
.

  !

Interfaces

! +
{ 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. / 
IProfileViewCallback/ C
)C D
,D E
SessionModeF Q
=R S
SessionModeT _
._ `
Required` h
)h i
]i j
public 

	interface 
IProfileViewManager (
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
GetProfileData 
( 
string "
nickname# +
)+ ,
;, -
} 
[ 
ServiceContract 
] 
public 

	interface  
IProfileViewCallback )
:* +
ISessionCallback, <
{ 
[ 	
OperationContract	 
] 
void 
ProfileDataReceived  
(  !
ServiceResponse! 0
<0 1
ProfileData1 <
>< =
response> F
)F G
;G H
} 
} ©
aC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IProfileEditManager.cs
	namespace

 	
UnoLisServer


 
.

 
	Contracts

  
.

  !

Interfaces

! +
{ 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. / 
IProfileEditCallback/ C
)C D
,D E
SessionModeF Q
=R S
SessionModeT _
._ `
Required` h
)h i
]i j
public 

	interface 
IProfileEditManager (
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
UpdateProfileData 
( 
ProfileData *
data+ /
)/ 0
;0 1
} 
[ 
ServiceContract 
] 
public 

	interface  
IProfileEditCallback )
:* +
ISessionCallback, <
{ 
[ 	
OperationContract	 
] 
void !
ProfileUpdateResponse "
(" #
ServiceResponse# 2
<2 3
ProfileData3 >
>> ?
response@ H
)H I
;I J
} 
} ­
_C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IPartyHostManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /
IPartyHostCallback/ A
)A B
,B C
SessionModeD O
=P Q
SessionModeR ]
.] ^
Required^ f
)f g
]g h
public 

	interface 
IPartyHostManager &
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
CreateParty 
( 
string 
hostNickname  ,
), -
;- .
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 

StartMatch 
( 
int 
partyId #
,# $
string% +
hostNickname, 8
)8 9
;9 :
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
CancelParty 
( 
int 
partyId $
)$ %
;% &
} 
[ 
ServiceContract 
] 
public 

	interface 
IPartyHostCallback '
:( )
ISessionCallback* :
{   
[!! 	
OperationContract!!	 
]!! 
void"" 
PartyCreated"" 
("" 
int"" 
partyId"" %
,""% &
string""' -
joinCode"". 6
)""6 7
;""7 8
[$$ 	
OperationContract$$	 
]$$ 
void%% 
PlayerJoined%% 
(%% 
string%%  
nickname%%! )
)%%) *
;%%* +
['' 	
OperationContract''	 
]'' 
void(( 

PlayerLeft(( 
((( 
string(( 
nickname(( '
)((' (
;((( )
[** 	
OperationContract**	 
]** 
void++ $
PlayerReadyStatusChanged++ %
(++% &
string++& ,
nickname++- 5
,++5 6
bool++7 ;
isReady++< C
)++C D
;++D E
[-- 	
OperationContract--	 
]-- 
void.. 
AllPlayersReady.. 
(.. 
).. 
;.. 
[00 	
OperationContract00	 
]00 
void11 
PartyCancelled11 
(11 
)11 
;11 
[33 	
OperationContract33	 
]33 
void44 
MatchStarted44 
(44 
)44 
;44 
}55 
}66 Ö
aC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IPartyClientManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. / 
IPartyClientCallback/ C
)C D
,D E
SessionModeF Q
=R S
SessionModeT _
._ `
Required` h
)h i
]i j
public 

	interface 
IPartyClientManager (
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
	JoinParty 
( 
JoinPartyRequest '
request( /
)/ 0
;0 1
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 

LeaveParty 
( 
PartyActionData '
data( ,
), -
;- .
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
SetReadyStatus 
( 
PartyActionData +
data, 0
)0 1
;1 2
} 
[ 
ServiceContract 
] 
public 

	interface  
IPartyClientCallback )
:* +
ISessionCallback, <
{   
[!! 	
OperationContract!!	 
]!! 
void"" 
JoinedSuccessfully"" 
(""  
int""  #
partyId""$ +
,""+ ,
string""- 3
hostNickname""4 @
)""@ A
;""A B
[$$ 	
OperationContract$$	 
]$$ 
void%% 

JoinFailed%% 
(%% 
string%% 
reason%% %
)%%% &
;%%& '
['' 	
OperationContract''	 
]'' 
void(( 
PlayerJoined(( 
((( 
string((  
nickname((! )
)(() *
;((* +
[** 	
OperationContract**	 
]** 
void++ 

PlayerLeft++ 
(++ 
string++ 
nickname++ '
)++' (
;++( )
[-- 	
OperationContract--	 
]-- 
void.. $
PlayerReadyStatusChanged.. %
(..% &
string..& ,
nickname..- 5
,..5 6
bool..7 ;
isReady..< C
)..C D
;..D E
[00 	
OperationContract00	 
]00 
void11 
MatchStarting11 
(11 
)11 
;11 
[33 	
OperationContract33	 
]33 
void44 
PartyCancelled44 
(44 
)44 
;44 
[66 	
OperationContract66	 
]66 
void77 
PartyDisbanded77 
(77 
)77 
;77 
}88 
}99 ú

cC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\INotificationsManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /"
INotificationsCallback/ E
)E F
,F G
SessionModeH S
=T U
SessionModeV a
.a b
Requiredb j
)j k
]k l
public 

	interface !
INotificationsManager *
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
SendNotification 
( 
NotificationData .
data/ 3
)3 4
;4 5
} 
[ 
ServiceContract 
] 
public 

	interface "
INotificationsCallback +
:, -
ISessionCallback. >
{ 
[ 	
OperationContract	 
] 
void  
NotificationReceived !
(! "
NotificationData" 2
data3 7
)7 8
;8 9
} 
} °

\C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\ILogoutManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /
ILogoutCallback/ >
)> ?
)? @
]@ A
public 

	interface 
ILogoutManager #
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
Logout 
( 
string 
nickname #
)# $
;$ %
} 
[ 
ServiceContract 
] 
public 

	interface 
ILogoutCallback $
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
LogoutResponse 
( 
ServiceResponse +
<+ ,
object, 2
>2 3
response4 <
)< =
;= >
} 
} §
WC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\PartyActionData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
PartyActionData		  
{

 
public 
int 
PartyId 
{ 
get  
;  !
set" %
;% &
}' (
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
bool 
? 
IsReady 
{ 
get "
;" #
set$ '
;' (
}) *
} 
} ž
RC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\FriendData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 

FriendData		 
{

 
public 
string 
FriendNickname $
{% &
get' *
;* +
set, /
;/ 0
}1 2
public 
bool 
IsOnline 
{ 
get "
;" #
set$ '
;' (
}) *
public 
string 
StatusMessage #
{$ %
get& )
;) *
set+ .
;. /
}0 1
} 
} ê	
WC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\ChatMessageData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{		 
public

 

class

 
ChatMessageData

  
{ 
[ 	

DataMember	 
] 
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
[ 	

DataMember	 
] 
public 
string 
Message 
{ 
get  #
;# $
set% (
;( )
}* +
[ 	

DataMember	 
] 
public 
DateTime 
	Timestamp !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
DateTime2 :
.: ;
Now; >
;> ?
[ 	

DataMember	 
] 
public 
string 
	ChannelId 
{  !
get" %
;% &
set' *
;* +
}, -
} 
} ƒ
[C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\ILoginManager.cs
	namespace

 	
UnoLisServer


 
.

 
	Contracts

  
.

  !

Interfaces

! +
{ 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /
ILoginCallback/ =
)= >
,> ?
SessionMode@ K
=L M
SessionModeN Y
.Y Z
RequiredZ b
)b c
]c d
public 

	interface 
ILoginManager "
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
Login 
( 
AuthCredentials "
credentials# .
). /
;/ 0
} 
[ 
ServiceContract 
] 
public 

	interface 
ILoginCallback #
:$ %
ISessionCallback& 6
{ 
[ 	
OperationContract	 
] 
void 
LoginResponse 
( 
ServiceResponse *
<* +
object+ 1
>1 2
response3 ;
); <
;< =
} 
} Ù
bC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\ILeaderBoardsManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /!
ILeaderboardsCallback/ D
)D E
,E F
SessionModeG R
=S T
SessionModeU `
.` a
Requireda i
)i j
]j k
public 

	interface  
ILeaderboardsManager )
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
GetLeaderboard 
( 
) 
; 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
GetPlayerRank 
( 
string !
nickname" *
)* +
;+ ,
} 
[ 
ServiceContract 
] 
public 

	interface !
ILeaderboardsCallback *
:+ ,
ISessionCallback- =
{ 
[ 	
OperationContract	 
] 
void 
LeaderboardReceived  
(  !
List! %
<% &
LeaderboardEntry& 6
>6 7
entries8 ?
)? @
;@ A
[ 	
OperationContract	 
] 
void 
PlayerRankReceived 
(  
LeaderboardEntry  0
entry1 6
)6 7
;7 8
} 
} …
^C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IGameplayManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /
IGameplayCallback/ @
)@ A
,A B
SessionModeC N
=O P
SessionModeQ \
.\ ]
Required] e
)e f
]f g
public 

	interface 
IGameplayManager %
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
PlayCard 
( 
PlayCardData "
data# '
)' (
;( )
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
DrawCard 
( 
string 
nickname %
)% &
;& '
} 
[ 
ServiceContract 
] 
public 

	interface 
IGameplayCallback &
:' (
ISessionCallback) 9
{ 
[ 	
OperationContract	 
] 
void 

CardPlayed 
( 
string 
nickname '
,' (
Card) -
card. 2
)2 3
;3 4
[ 	
OperationContract	 
] 
void 
	CardDrawn 
( 
string 
nickname &
)& '
;' (
[ 	
OperationContract	 
] 
void 
TurnChanged 
( 
string 
nextPlayerNickname  2
)2 3
;3 4
[!! 	
OperationContract!!	 
]!! 
void"" 

MatchEnded"" 
("" 
List"" 
<"" 

ResultData"" '
>""' (
results"") 0
)""0 1
;""1 2
}$$ 
}%% ´
]C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IFriendsManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /
IFriendsCallback/ ?
)? @
,@ A
SessionModeB M
=N O
SessionModeP [
.[ \
Required\ d
)d e
]e f
public 

	interface 
IFriendsManager $
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
GetFriendsList 
( 
string "
nickname# +
)+ ,
;, -
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
SendFriendRequest 
( 
FriendRequestData 0
request1 8
)8 9
;9 :
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
AcceptFriendRequest  
(  !
FriendRequestData! 2
request3 :
): ;
;; <
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
RemoveFriend 
( 
FriendRequestData +
request, 3
)3 4
;4 5
} 
[ 
ServiceContract 
] 
public 

	interface 
IFriendsCallback %
:& '
ISessionCallback( 8
{ 
[ 	
OperationContract	 
] 
void 
FriendsListReceived  
(  !
List! %
<% &

FriendData& 0
>0 1
friends2 9
)9 :
;: ;
[!! 	
OperationContract!!	 
]!! 
void"" !
FriendRequestReceived"" "
(""" #
string""# )
fromNickname""* 6
)""6 7
;""7 8
[$$ 	
OperationContract$$	 
]$$ 
void%% 
FriendRequestResult%%  
(%%  !
bool%%! %
success%%& -
,%%- .
string%%/ 5
message%%6 =
)%%= >
;%%> ?
['' 	
OperationContract''	 
]'' 
void(( 
FriendListUpdated(( 
((( 
List(( #
<((# $

FriendData(($ .
>((. /
updatedList((0 ;
)((; <
;((< =
})) 
}** ï
bC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IConfirmationManager.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !

Interfaces		! +
{

 
[ 
ServiceContract 
( 
CallbackContract %
=& '
typeof( .
(. /!
IConfirmationCallback/ D
)D E
,E F
SessionModeG R
=S T
SessionModeU `
.` a
Requireda i
)i j
]j k
public 

	interface  
IConfirmationManager )
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
ConfirmCode 
( 
string 
email  %
,% &
string' -
code. 2
)2 3
;3 4
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void "
ResendConfirmationCode #
(# $
string$ *
email+ 0
)0 1
;1 2
} 
[ 
ServiceContract 
] 
public 

	interface !
IConfirmationCallback *
:+ ,
ISessionCallback- =
{ 
[ 	
OperationContract	 
] 
void  
ConfirmationResponse !
(! "
ServiceResponse" 1
<1 2
object2 8
>8 9
response: B
)B C
;C D
[ 	
OperationContract	 
] 
void 
ResendCodeResponse 
(  
ServiceResponse  /
</ 0
object0 6
>6 7
response8 @
)@ A
;A B
} 
} —
ZC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Interfaces\IChatManager.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !

Interfaces! +
{ 
[ 
ServiceContract 
( 
CallbackContract 
= 
typeof !
(! "
IChatCallback" /
)/ 0
,0 1
SessionMode		 
=		 
SessionMode		 !
.		! "
Required		" *
)		* +
]		+ ,
public

 

	interface

 
IChatManager

 !
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
SendMessage 
( 
ChatMessageData (
message) 0
)0 1
;1 2
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
GetChatHistory 
( 
string "
	channelId# ,
), -
;- .
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
RegisterPlayer 
( 
string "
nickname# +
)+ ,
;, -
} 
[ 
ServiceContract 
] 
public 

	interface 
IChatCallback "
:# $
ISessionCallback% 5
{ 
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
MessageReceived 
( 
ChatMessageData ,
message- 4
)4 5
;5 6
[ 	
OperationContract	 
( 
IsOneWay #
=$ %
true& *
)* +
]+ ,
void 
ChatHistoryReceived  
(  !
ChatMessageData! 0
[0 1
]1 2
messages3 ;
); <
;< =
} 
} Á
SC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Enums\RarityType.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
Enums! &
{ 
public		 

enum		 

RarityType		 
{

 
Common 
, 
Special 
, 
Epic 
, 
	Legendary 
} 
} í
UC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Enums\PlayerStatus.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
Enums! &
{ 
public		 

enum		 
PlayerStatus		 
{

 
Offline 
, 
Online 
, 
InLobby 
, 
InMatch 
, 

Spectating 
} 
} »
YC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Enums\NotificationType.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
Enums! &
{ 
public		 

enum		 
NotificationType		  
{

 
FriendRequest 
, 
MatchInvite 
, 
SystemMessage 
} 
} ®
RC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Enums\CardValue.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
Enums! &
{ 
public		 

enum		 
	CardValue		 
{

 
Zero 
, 
One 
, 
Two 
, 
Three 
, 
Four 
, 
Five 
, 
Six 
, 
Seven 
, 
Eight 
, 
Nine 
, 
Skip 
, 
Reverse 
, 
DrawTwo 
, 
Wild 
, 
WildDrawFour 
} 
} Ø
RC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\Enums\CardColor.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
Enums! &
{ 
public		 

enum		 
	CardColor		 
{

 
Red 
, 
Yellow 
, 
Green 
, 
Blue 
, 
Wild 
} 
} ƒ
WC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\AuthCredentials.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
AuthCredentials		  
{

 
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
string 
Password 
{  
get! $
;$ %
set& )
;) *
}+ ,
} 
} ß
SC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\ProfileData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
ProfileData		 
{

 
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
string 
FullName 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
string 
Password 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
string 
Email 
{ 
get !
;! "
set# &
;& '
}( )
public 
int 
Level 
{ 
get 
; 
set  #
;# $
}% &
public 
int 
ExperiencePoints #
{$ %
get& )
;) *
set+ .
;. /
}0 1
public 
int 
MatchesPlayed  
{! "
get# &
;& '
set( +
;+ ,
}- .
public 
int 
Wins 
{ 
get 
; 
set "
;" #
}$ %
public 
int 
Losses 
{ 
get 
;  
set! $
;$ %
}& '
public 
int 
Streak 
{ 
get 
;  
set! $
;$ %
}& '
public 
int 
	MaxStreak 
{ 
get "
;" #
set$ '
;' (
}) *
public 
string 
CurrentAvatar #
{$ %
get& )
;) *
set+ .
;. /
}0 1
public 
string 
FacebookUrl !
{" #
get$ '
;' (
set) ,
;, -
}. /
public 
string 
InstagramUrl "
{# $
get% (
;( )
set* -
;- .
}/ 0
public 
string 
	TikTokUrl 
{  !
get" %
;% &
set' *
;* +
}, -
} 
} ¹
XC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\NotificationType.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

enum		 
NotificationType		  
{

 
FriendRequest 
, 
MatchInvite 
, 
SystemMessage 
} 
} º
PC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\ShopItem.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
ShopItem		 
{

 
public 
int 
BoxId 
{ 
get 
; 
set  #
;# $
}% &
public 
string 
Name 
{ 
get  
;  !
set" %
;% &
}' (
public 
string 
Description !
{" #
get$ '
;' (
set) ,
;, -
}. /
public 
string 
Rarity 
{ 
get "
;" #
set$ '
;' (
}) *
public 
int 
Price 
{ 
get 
; 
set  #
;# $
}% &
} 
} ¦
RC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\ResultData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 

ResultData		 
{

 
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
int 
Score 
{ 
get 
; 
set  #
;# $
}% &
public 
int 
Position 
{ 
get !
;! "
set# &
;& '
}( )
public 
bool 
IsWinner 
{ 
get "
;" #
set$ '
;' (
}) *
} 
} þ
WC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\PurchaseRequest.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
PurchaseRequest		  
{

 
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
int 
ItemId 
{ 
get 
;  
set! $
;$ %
}& '
} 
} º
XC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\RegistrationData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
RegistrationData		 !
{

 
public 
string 
Email 
{ 
get !
;! "
set# &
;& '
}( )
public 
string 
Password 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
string 
FullName 
{  
get! $
;$ %
set& )
;) *
}+ ,
} 
} ý
TC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\PlayCardData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
PlayCardData		 
{

 
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
Card 

PlayedCard 
{  
get! $
;$ %
set& )
;) *
}+ ,
} 
} þ
YC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\NetworkUpdateData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
NetworkUpdateData		 "
{

 
public 
string 
Type 
{ 
get  
;  !
set" %
;% &
}' (
public 
string 
Url 
{ 
get 
;  
set! $
;$ %
}& '
} 
} ™
XC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\NotificationData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
NotificationData		 !
{

 
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
string 
Message 
{ 
get  #
;# $
set% (
;( )
}* +
public 
NotificationType 
Type  $
{% &
get' *
;* +
set, /
;/ 0
}1 2
public 
DateTime 
SentAt 
{  
get! $
;$ %
set& )
;) *
}+ ,
=- .
DateTime/ 7
.7 8
UtcNow8 >
;> ?
} 
} ê	
XC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\LeaderboardEntry.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
LeaderboardEntry		 !
{

 
public 
int 
Rank 
{ 
get 
; 
set "
;" #
}$ %
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
int 
Wins 
{ 
get 
; 
set "
;" #
}$ %
public 
int 
MatchesPlayed  
{! "
get# &
;& '
set( +
;+ ,
}- .
public 
int 
GlobalPoints 
{  !
get" %
;% &
set' *
;* +
}, -
public 
string 
FullName 
{  
get! $
;$ %
set& )
;) *
}+ ,
} 
} …
XC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\JoinPartyRequest.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
JoinPartyRequest		 !
{

 
public 
string 
Nickname 
{  
get! $
;$ %
set& )
;) *
}+ ,
public 
string 
JoinCode 
{  
get! $
;$ %
set& )
;) *
}+ ,
} 
} –
YC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\FriendRequestData.cs
	namespace 	
UnoLisServer
 
. 
	Contracts  
.  !
DTOs! %
{ 
public		 

class		 
FriendRequestData		 "
{

 
public 
string 
RequesterNickname '
{( )
get* -
;- .
set/ 2
;2 3
}4 5
public 
string 
TargetNickname $
{% &
get' *
;* +
set, /
;/ 0
}1 2
} 
} ‚
LC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Contracts\DTOs\Card.cs
	namespace		 	
UnoLisServer		
 
.		 
	Contracts		  
.		  !
DTOs		! %
{

 
[ 
DataContract 
] 
public 

class 
Card 
{ 
[ 	

DataMember	 
] 
public 
	CardColor 
Color 
{  
get! $
;$ %
set& )
;) *
}+ ,
[ 	

DataMember	 
] 
public 
	CardValue 
Value 
{  
get! $
;$ %
set& )
;) *
}+ ,
[ 	

DataMember	 
] 
public 
string 
	ImagePath 
{  !
get" %
;% &
set' *
;* +
}, -
[ 	

DataMember	 
] 
public 
string 
Description !
{" #
get$ '
;' (
set) ,
;, -
}. /
public 
Card 
( 
) 
{ 
} 
public 
Card 
( 
	CardColor 
color #
,# $
	CardValue% .
value/ 4
,4 5
string6 <
	imagePath= F
=G H
nullI M
,M N
stringO U
descriptionV a
=b c
nulld h
)h i
{ 	
Color 
= 
color 
; 
Value 
= 
value 
; 
	ImagePath   
=   
	imagePath   !
;  ! "
Description!! 
=!! 
description!! %
;!!% &
}"" 	
public$$ 
override$$ 
string$$ 
ToString$$ '
($$' (
)$$( )
{%% 	
return&& 
$"&& 
{&& 
Color&& 
}&& 
$str&& 
{&& 
Value&& #
}&&# $
"&&$ %
;&&% &
}'' 	
}(( 
})) 