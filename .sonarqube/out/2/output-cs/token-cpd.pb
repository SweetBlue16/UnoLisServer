‚
WC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Properties\AssemblyInfo.cs
[ 
assembly 	
:	 

AssemblyTitle 
( 
$str .
). /
]/ 0
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
$str 0
)0 1
]1 2
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
]!!) *ô
VC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Models\ServiceResponse.cs
	namespace		 	
UnoLisServer		
 
.		 
Common		 
.		 
Models		 $
{

 
[ 
DataContract 
] 
public 

class 
ServiceResponse  
<  !
T! "
>" #
{ 
[ 	

DataMember	 
] 
public 
bool 
Success 
{ 
get !
;! "
set# &
;& '
}( )
[ 	

DataMember	 
] 
public 
MessageCode 
Code 
{  !
get" %
;% &
set' *
;* +
}, -
[ 	

DataMember	 
] 
public 
T 
Data 
{ 
get 
; 
set  
;  !
}" #
public 
ServiceResponse 
( 
)  
{! "
}# $
public 
ServiceResponse 
( 
bool #
success$ +
,+ ,
MessageCode- 8
code9 =
,= >
T? @
dataA E
=F G
defaultH O
)O P
{ 	
Success 
= 
success 
; 
Code 
= 
code 
; 
Data 
= 
data 
; 
} 	
} 
}   ¼
VC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Models\OperationResult.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
Models $
{ 
public		 

class		 
OperationResult		  
{

 
public 
bool 
Success 
{ 
get !
;! "
set# &
;& '
}( )
public 
string 
Message 
{ 
get  #
;# $
set% (
;( )
}* +
public 
static 
OperationResult %
Ok& (
(( )
string) /
msg0 3
=4 5
$str6 I
)I J
=>K M
new 
OperationResult 
{  !
Success" )
=* +
true, 0
,0 1
Message2 9
=: ;
msg< ?
}@ A
;A B
public 
static 
OperationResult %
Fail& *
(* +
string+ 1
msg2 5
=6 7
$str8 O
)O P
=>Q S
new 
OperationResult 
{  !
Success" )
=* +
false, 1
,1 2
Message3 :
=; <
msg= @
}A B
;B C
} 
} ¦
WC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Helpers\PasswordHelpers.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
Helpers %
{		 
public

 

static

 
class

 
PasswordHelper

 &
{ 
public 
static 
string 
HashPassword )
() *
string* 0
password1 9
)9 :
{ 	
using 
( 
SHA256 
sha 
= 
SHA256  &
.& '
Create' -
(- .
). /
)/ 0
{ 
byte 
[ 
] 
bytes 
= 
sha "
." #
ComputeHash# .
(. /
Encoding/ 7
.7 8
UTF88 <
.< =
GetBytes= E
(E F
passwordF N
)N O
)O P
;P Q
StringBuilder 
sb  
=! "
new# &
StringBuilder' 4
(4 5
)5 6
;6 7
foreach 
( 
byte 
b 
in  "
bytes# (
)( )
{ 
sb 
. 
Append 
( 
b 
.  
ToString  (
(( )
$str) -
)- .
). /
;/ 0
} 
return 
sb 
. 
ToString "
(" #
)# $
;$ %
} 
} 	
public 
static 
bool 
VerifyPassword )
() *
string* 0
	plainText1 :
,: ;
string< B
hashedC I
)I J
{ 	
return 
HashPassword 
(  
	plainText  )
)) *
==+ -
hashed. 4
;4 5
} 	
} 
} 
NC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Helpers\Logger.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
Helpers %
{ 
public 

static 
class 
Logger 
{ 
private 
static 
readonly 
string  &
logFile' .
=/ 0
	Constants1 :
.: ;
	Constants; D
.D E
LogFileNameE P
;P Q
public 
static 
void 
Log 
( 
string %
message& -
)- .
{ 	
string 
entry 
= 
$" 
$str 
{ 
DateTime '
.' (
Now( +
:+ ,
$str, ?
}? @
$str@ B
{B C
messageC J
}J K
"K L
;L M
Console 
. 
	WriteLine 
( 
entry #
)# $
;$ %
try 
{ 
File 
. 
AppendAllText "
(" #
logFile# *
,* +
entry, 1
+2 3
Environment4 ?
.? @
NewLine@ G
)G H
;H I
} 
catch 
( 
IOException 
) 
{ 
Console 
. 
	WriteLine !
(! "
$str" N
)N O
;O P
} 
} 	
} 
} ö5
QC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Enums\MessageCode.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
Enums #
{ 
public		 

enum		 
MessageCode		 
{

 
Success 
= 
$num 
, "
RegistrationSuccessful 
=  
$num! %
,% &
LoginSuccessful 
= 
$num 
, 
LogoutSuccessful 
= 
$num 
,   
ProfileDataRetrieved 
= 
$num #
,# $
ProfileUpdated 
= 
$num 
, 
FriendRequestSent 
= 
$num  
,  !!
FriendRequestAccepted 
= 
$num  $
,$ %
FriendRemoved 
= 
$num 
, 
ChatMessageSent 
= 
$num 
,  
ChatMessageRetrieved 
= 
$num #
,# $
LobbyCreated 
= 
$num 
, 
LobbyJoined 
= 
$num 
, 
	LobbyLeft 
= 
$num 
, 
MatchStarted 
= 
$num 
, 

MatchEnded 
= 
$num 
, 

BadRequest 
= 
$num 
, 
InvalidData 
= 
$num 
, 
InvalidCredentials   
=   
$num   !
,  ! " 
NicknameAlreadyTaken!! 
=!! 
$num!! #
,!!# $"
EmailAlreadyRegistered"" 
=""  
$num""! %
,""% &
InvalidEmailFormat## 
=## 
$num## !
,##! "
WeakPassword$$ 
=$$ 
$num$$ 
,$$ 
PlayerNotFound%% 
=%% 
$num%% 
,%% 
SamePassword&& 
=&& 
$num&& 
,&& 
EmptyMessage'' 
='' 
$num'' 
,'' 
MessageTooLong(( 
=(( 
$num(( 
,((  
InappropriateContent)) 
=)) 
$num)) #
,))# $
AlreadyFriends** 
=** 
$num** 
,**  
PendingFriendRequest++ 
=++ 
$num++ #
,++# $
InvalidSocialUrl,, 
=,, 
$num,, 
,,,  
BlockedUser-- 
=-- 
$num-- 
,-- 
LobbyNotFound.. 
=.. 
$num.. 
,.. 
	LobbyFull// 
=// 
$num// 
,// !
OperationNotSupported00 
=00 
$num00  $
,00$ %
ValidationFailed11 
=11 
$num11 
,11  
EmptyFields22 
=22 
$num22 
,22 
SessionExpired55 
=55 
$num55 
,55 
UnauthorizedAccess66 
=66 
$num66 !
,66! "
InvalidToken77 
=77 
$num77 
,77 
MissingToken88 
=88 
$num88 
,88 
DuplicateSession99 
=99 
$num99 
,99  
UserNotConnected:: 
=:: 
$num:: 
,::  
LoginInternalError;; 
=;; 
$num;; !
,;;! "
LogoutInternalError<< 
=<< 
$num<< "
,<<" #
DatabaseError?? 
=?? 
$num?? 
,?? 
TransactionFailed@@ 
=@@ 
$num@@  
,@@  !
SqlErrorAA 
=AA 
$numAA 
,AA 
ConcurrencyConflictBB 
=BB 
$numBB "
,BB" #
SerializationErrorCC 
=CC 
$numCC !
,CC! "
UnhandledExceptionDD 
=DD 
$numDD !
,DD! "
CallbackErrorEE 
=EE 
$numEE 
,EE 
ProfileUpdateFailedFF 
=FF 
$numFF "
,FF" #
ProfileFetchFailedGG 
=GG 
$numGG !
,GG! "
ChatInternalErrorHH 
=HH 
$numHH  
,HH  ! 
FriendsInternalErrorII 
=II 
$numII #
,II# $
LobbyInternalErrorJJ 
=JJ 
$numJJ !
,JJ! "
GeneralServerErrorKK 
=KK 
$numKK !
,KK! "
ConnectionLostNN 
=NN 
$numNN 
,NN 
TimeoutOO 
=OO 
$numOO 
,OO 
ConnectionFailedPP 
=PP 
$numPP 
,PP  
ConnectionRejectedQQ 
=QQ 
$numQQ !
,QQ! "
UnstableConnectionRR 
=RR 
$numRR !
,RR! "
ClientDisconnectedSS 
=SS 
$numSS !
,SS! "!
FriendActionCompletedVV 
=VV 
$numVV  $
,VV$ %
PlayerBlockedWW 
=WW 
$numWW 
,WW 
PlayerUnblockedXX 
=XX 
$numXX 
,XX  
PlayerHasActiveLobbyYY 
=YY 
$numYY #
,YY# $
PlayerNotInLobbyZZ 
=ZZ 
$numZZ 
,ZZ  
PlayerAlreadyReady[[ 
=[[ 
$num[[ !
,[[! "
PlayerNotReady\\ 
=\\ 
$num\\ 
,\\ 
MatchAlreadyStarted]] 
=]] 
$num]] "
,]]" #
MatchCancelled^^ 
=^^ 
$num^^ 
,^^ 
MatchNotFound__ 
=__ 
$num__ 
,__ 
MatchAlreadyEnded`` 
=`` 
$num``  
,``  !
PlayerKickedaa 
=aa 
$numaa 
,aa 
PlayerBannedbb 
=bb 
$numbb 
,bb 
LobbyClosedcc 
=cc 
$numcc 
,cc 
NoPermissionsdd 
=dd 
$numdd 
,dd "
LobbyInconsistentStateee 
=ee  
$numee! %
,ee% &
PlayerDisconnectedff 
=ff 
$numff !
,ff! "
PlayerReconnectedgg 
=gg 
$numgg  
,gg  ! 
MatchResultsRecordedhh 
=hh 
$numhh #
,hh# $!
RewardProcessingErrorii 
=ii 
$numii  $
,ii$ %#
PurchaseProcessingErrorjj 
=jj  !
$numjj" &
}kk 
}ll ž
SC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Constants\Endpoints.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
	Constants '
{ 
public		 

static		 
class		 
	Endpoints		 !
{

 
public 
const 
string 
AuthEndpoint (
=) *
$str+ J
;J K
public 
const 
string 
ProfileEndpoint +
=, -
$str. P
;P Q
public 
const 
string 
FriendsEndpoint +
=, -
$str. P
;P Q
public 
const 
string 
ShopEndpoint (
=) *
$str+ J
;J K
public 
const 
string 
PartyHostEndpoint -
=. /
$str0 T
;T U
public 
const 
string 
PartyClientEndpoint /
=0 1
$str2 X
;X Y
public 
const 
string 
GameplayEndpoint ,
=- .
$str/ R
;R S
public 
const 
string !
NotificationsEndpoint 1
=2 3
$str4 \
;\ ]
}   
}!! ƒ
SC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Constants\Constants.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
	Constants '
{ 
public		 

static		 
class		 
	Constants		 !
{

 
public 
const 
string '
DefaultConnectionStringName 7
=8 9
$str: F
;F G
public 
const 
int 
MaxPlayersPerLobby +
=, -
$num. /
;/ 0
public 
const 
int 
MaxFriendRequests *
=+ ,
$num- 0
;0 1
public 
const 
int 
CardDrawDelayMs (
=) *
$num+ .
;. /
public 
const 
string 
LogFileName '
=( )
$str* <
;< =
} 
} ­
UC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Enums\ConnectionState.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
Enums #
{ 
public		 

enum		 
ConnectionState		 
{

 
Disconnected 
, 

Connecting 
, 
	Connected 
} 
} ¤
WC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Config\EnviromentConfig.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
Config $
{ 
public		 

static		 
class		 
EnvironmentConfig		 )
{

 
public 
static 
string 
DbUser #
=>$ &
Environment' 2
.2 3"
GetEnvironmentVariable3 I
(I J
$strJ W
)W X
??Y [
$str\ e
;e f
public 
static 
string 

DbPassword '
=>( *
Environment+ 6
.6 7"
GetEnvironmentVariable7 M
(M N
$strN [
)[ \
??] _
$str` j
;j k
public 
static 
string 
DbServer %
=>& (
Environment) 4
.4 5"
GetEnvironmentVariable5 K
(K L
$strL [
)[ \
??] _
$str` i
;i j
public 
static 
string 
DbName #
=>$ &
Environment' 2
.2 3"
GetEnvironmentVariable3 I
(I J
$strJ W
)W X
??Y [
$str\ i
;i j
public 
static 
string !
BuildConnectionString 2
(2 3
)3 4
{ 	
return 
$" 
$str !
{! "
DbServer" *
}* +
$str+ <
{< =
DbName= C
}C D
$strD M
{M N
DbUserN T
}T U
$strU _
{_ `

DbPassword` j
}j k
$str	k ¦
"
¦ §
;
§ ¨
} 	
} 
} 