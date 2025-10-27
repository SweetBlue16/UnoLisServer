Ñ
MC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\ShopManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
ShopManager 
: 
IShopManager +
{ 
private 
readonly 

UNOContext #
_context$ ,
;, -
private 
readonly 
IShopCallback &
	_callback' 0
;0 1
public 
ShopManager 
( 
) 
{ 	
_context 
= 
new 

UNOContext %
(% &
)& '
;' (
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
IShopCallbackD Q
>Q R
(R S
)S T
;T U
} 	
public 
void 
GetShopItems  
(  !
)! "
{ 	
var 
items 
= 
_context  
.  !
LootBoxType! ,
. 
Select 
( 
i 
=> 
new  
ShopItem! )
{ 
BoxId 
= 
i 
. 
idLootBoxType +
,+ ,
Name   
=   
i   
.   
boxName   $
,  $ %
Price!! 
=!! 
i!! 
.!! 
price!! #
,!!# $
Description"" 
=""  !
i""" #
.""# $
description""$ /
}## 
)## 
.## 
ToList## 
(## 
)## 
;## 
	_callback%% 
.%% 
ShopItemsReceived%% '
(%%' (
items%%( -
)%%- .
;%%. /
}&& 	
public(( 
void(( 
PurchaseItem((  
(((  !
PurchaseRequest((! 0
request((1 8
)((8 9
{)) 	
	_callback** 
.** 
PurchaseResponse** &
(**& '
true**' +
,**+ ,
$"**- /
$str**/ F
{**F G
request**G N
.**N O
ItemId**O U
}**U V
"**V W
)**W X
;**X Y
}++ 	
},, 
}-- È
PC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\SessionManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
public 

static 
class 
SessionManager &
{ 
private		 
static		 
readonly		  
ConcurrentDictionary		  4
<		4 5
string		5 ;
,		; <
ISessionCallback		= M
>		M N
ActiveSessions		O ]
=		^ _
new

  
ConcurrentDictionary

  
<

  !
string

! '
,

' (
ISessionCallback

) 9
>

9 :
(

: ;
)

; <
;

< =
public 
static 
void 

AddSession %
(% &
string& ,
nickname- 5
,5 6
ISessionCallback7 G
callbackH P
)P Q
{ 	
ActiveSessions 
[ 
nickname #
]# $
=% &
callback' /
;/ 0
} 	
public 
static 
void 
RemoveSession (
(( )
string) /
nickname0 8
)8 9
{ 	
ActiveSessions 
. 
	TryRemove $
($ %
nickname% -
,- .
out/ 2
_3 4
)4 5
;5 6
} 	
public 
static 
ISessionCallback &

GetSession' 1
(1 2
string2 8
nickname9 A
)A B
{ 	
ActiveSessions 
. 
TryGetValue &
(& '
nickname' /
,/ 0
out1 4
var5 8
callback9 A
)A B
;B C
return 
callback 
; 
} 	
public 
static 
bool 
IsOnline #
(# $
string$ *
nickname+ 3
)3 4
{ 	
return 
ActiveSessions !
.! "
ContainsKey" -
(- .
nickname. 6
)6 7
;7 8
}   	
}!! 
}"" ’P
QC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\RegisterManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
RegisterManager  
:! "
IRegisterManager# 3
{ 
private 
readonly 

UNOContext #
_context$ ,
;, -
private 
readonly 
IRegisterCallback *
	_callback+ 4
;4 5
private 
ServiceResponse 
<  
object  &
>& '
	_response( 1
;1 2
private 
readonly 
INotificationSender ,
_notificationSender- @
;@ A
private 
readonly #
IVerificationCodeHelper 0#
_verificationCodeHelper1 H
;H I
private 
readonly &
IPendingRegistrationHelper 3&
_pendingRegistrationHelper4 N
;N O
public 
RegisterManager 
( 
)  
{ 	
_context   
=   
new   

UNOContext   %
(  % &
)  & '
;  ' (
	_callback!! 
=!! 
OperationContext!! (
.!!( )
Current!!) 0
.!!0 1
GetCallbackChannel!!1 C
<!!C D
IRegisterCallback!!D U
>!!U V
(!!V W
)!!W X
;!!X Y
_notificationSender## 
=##  !
NotificationSender##" 4
.##4 5
Instance##5 =
;##= >#
_verificationCodeHelper$$ #
=$$$ %"
VerificationCodeHelper$$& <
.$$< =
Instance$$= E
;$$E F&
_pendingRegistrationHelper%% &
=%%' (%
PendingRegistrationHelper%%) B
.%%B C
Instance%%C K
;%%K L
}&& 	
public(( 
void(( 
Register(( 
((( 
RegistrationData(( -
data((. 2
)((2 3
{)) 	
if** 
(** 
data** 
==** 
null** 
)** 
{++ 
	_response,, 
=,, 
new,, 
ServiceResponse,,  /
<,,/ 0
object,,0 6
>,,6 7
(,,7 8
false,,8 =
,,,= >
MessageCode,,? J
.,,J K
InvalidData,,K V
),,V W
;,,W X
	_callback-- 
.-- 
RegisterResponse-- *
(--* +
	_response--+ 4
)--4 5
;--5 6
return.. 
;.. 
}// 
try11 
{22 
Logger33 
.33 
Log33 
(33 
$"33 
$str33 >
{33> ?
data33? C
.33C D
Nickname33D L
}33L M
$str33M Q
"33Q R
)33R S
;33S T
bool55 
existsPlayer55 !
=55" #
_context55$ ,
.55, -
Player55- 3
.553 4
Any554 7
(557 8
p558 9
=>55: <
p55= >
.55> ?
nickname55? G
==55H J
data55K O
.55O P
Nickname55P X
)55X Y
;55Y Z
if66 
(66 
existsPlayer66  
)66  !
{77 
	_response88 
=88 
new88  #
ServiceResponse88$ 3
<883 4
object884 :
>88: ;
(88; <
false88< A
,88A B
MessageCode88C N
.88N O 
NicknameAlreadyTaken88O c
)88c d
;88d e
	_callback99 
.99 
RegisterResponse99 .
(99. /
	_response99/ 8
)998 9
;999 :
Logger:: 
.:: 
Log:: 
(:: 
$":: !
$str::! .
{::. /
data::/ 3
.::3 4
Nickname::4 <
}::< =
$str::= R
"::R S
)::S T
;::T U
return;; 
;;; 
}<< 
bool>> 
existsAccount>> "
=>># $
_context>>% -
.>>- .
Account>>. 5
.>>5 6
Any>>6 9
(>>9 :
a>>: ;
=>>>< >
a>>? @
.>>@ A
email>>A F
==>>G I
data>>J N
.>>N O
Email>>O T
)>>T U
;>>U V
if?? 
(?? 
existsAccount?? !
)??! "
{@@ 
	_responseAA 
=AA 
newAA  #
ServiceResponseAA$ 3
<AA3 4
objectAA4 :
>AA: ;
(AA; <
falseAA< A
,AAA B
MessageCodeAAC N
.AAN O"
EmailAlreadyRegisteredAAO e
)AAe f
;AAf g
	_callbackBB 
.BB 
RegisterResponseBB .
(BB. /
	_responseBB/ 8
)BB8 9
;BB9 :
LoggerCC 
.CC 
LogCC 
(CC 
$"CC !
$strCC! +
{CC+ ,
dataCC, 0
.CC0 1
EmailCC1 6
}CC6 7
$strCC7 L
"CCL M
)CCM N
;CCN O
returnDD 
;DD 
}EE 
varGG 
pendingDataGG 
=GG  !
newGG" %
PendingRegistrationGG& 9
{HH 
NicknameII 
=II 
dataII #
.II# $
NicknameII$ ,
,II, -
FullNameJJ 
=JJ 
dataJJ #
.JJ# $
FullNameJJ$ ,
,JJ, -
HashedPasswordKK "
=KK# $
PasswordHelperKK% 3
.KK3 4
HashPasswordKK4 @
(KK@ A
dataKKA E
.KKE F
PasswordKKF N
)KKN O
}LL 
;LL &
_pendingRegistrationHelperNN *
.NN* +$
StorePendingRegistrationNN+ C
(NNC D
dataNND H
.NNH I
EmailNNI N
,NNN O
pendingDataNNP [
)NN[ \
;NN\ ]
varOO 
codeOO 
=OO #
_verificationCodeHelperOO 2
.OO2 3 
GenerateAndStoreCodeOO3 G
(OOG H
dataOOH L
.OOL M
EmailOOM R
,OOR S
CodeTypeOOT \
.OO\ ]
EmailVerificationOO] n
)OOn o
;OOo p
_notificationSenderPP #
.PP# $-
!SendAccountVerificationEmailAsyncPP$ E
(PPE F
dataPPF J
.PPJ K
EmailPPK P
,PPP Q
codePPR V
)PPV W
;PPW X
	_responseRR 
=RR 
newRR 
ServiceResponseRR  /
<RR/ 0
objectRR0 6
>RR6 7
(RR7 8
trueRR8 <
,RR< =
MessageCodeRR> I
.RRI J 
VerificationCodeSentRRJ ^
)RR^ _
;RR_ `
	_callbackSS 
.SS 
RegisterResponseSS *
(SS* +
	_responseSS+ 4
)SS4 5
;SS5 6
LoggerTT 
.TT 
LogTT 
(TT 
$"TT 
$strTT .
{TT. /
dataTT/ 3
.TT3 4
EmailTT4 9
}TT9 :
$strTT: U
"TTU V
)TTV W
;TTW X
}UU 
catchVV 
(VV "
CommunicationExceptionVV )
communicationExVV* 9
)VV9 :
{WW 
LoggerXX 
.XX 
LogXX 
(XX 
$"XX 
$strXX ?
{XX? @
dataXX@ D
.XXD E
EmailXXE J
}XXJ K
$strXXK N
{XXN O
communicationExXXO ^
.XX^ _
MessageXX_ f
}XXf g
"XXg h
)XXh i
;XXi j
	_responseYY 
=YY 
newYY 
ServiceResponseYY  /
<YY/ 0
objectYY0 6
>YY6 7
(YY7 8
falseYY8 =
,YY= >
MessageCodeYY? J
.YYJ K
ConnectionFailedYYK [
)YY[ \
;YY\ ]
	_callbackZZ 
.ZZ 
RegisterResponseZZ *
(ZZ* +
	_responseZZ+ 4
)ZZ4 5
;ZZ5 6
}[[ 
catch\\ 
(\\ 
TimeoutException\\ #
	timeoutEx\\$ -
)\\- .
{]] 
Logger^^ 
.^^ 
Log^^ 
(^^ 
$"^^ 
$str^^ 1
{^^1 2
data^^2 6
.^^6 7
Email^^7 <
}^^< =
$str^^= @
{^^@ A
	timeoutEx^^A J
.^^J K
Message^^K R
}^^R S
"^^S T
)^^T U
;^^U V
	_response__ 
=__ 
new__ 
ServiceResponse__  /
<__/ 0
object__0 6
>__6 7
(__7 8
false__8 =
,__= >
MessageCode__? J
.__J K
Timeout__K R
)__R S
;__S T
	_callback`` 
.`` 
RegisterResponse`` *
(``* +
	_response``+ 4
)``4 5
;``5 6
}aa 
catchbb 
(bb 
	Exceptionbb 
exbb 
)bb  
{cc 
Loggerdd 
.dd 
Logdd 
(dd 
$"dd 
$strdd /
{dd/ 0
datadd0 4
.dd4 5
Emaildd5 :
}dd: ;
$strdd; >
{dd> ?
exdd? A
.ddA B
MessageddB I
}ddI J
"ddJ K
)ddK L
;ddL M
	_responseee 
=ee 
newee 
ServiceResponseee  /
<ee/ 0
objectee0 6
>ee6 7
(ee7 8
falseee8 =
,ee= >
MessageCodeee? J
.eeJ K
GeneralServerErroreeK ]
)ee] ^
;ee^ _
	_callbackff 
.ff 
RegisterResponseff *
(ff* +
	_responseff+ 4
)ff4 5
;ff5 6
}gg 
}hh 	
}ii 
}jj “
VC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\NotificationsManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class  
NotificationsManager %
:& '!
INotificationsManager( =
{ 
private 
readonly "
INotificationsCallback /
	_callback0 9
;9 :
public  
NotificationsManager #
(# $
)$ %
{ 	
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D"
INotificationsCallbackD Z
>Z [
([ \
)\ ]
;] ^
} 	
public 
void 
SendNotification $
($ %
NotificationData% 5
data6 :
): ;
{ 	
	_callback 
.  
NotificationReceived *
(* +
data+ /
)/ 0
;0 1
} 	
} 
} Ñ
YC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\Properties\AssemblyInfo.cs
[ 
assembly 	
:	 

AssemblyTitle 
( 
$str 0
)0 1
]1 2
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
$str 2
)2 3
]3 4
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
]!!) *´U
TC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\ProfileViewManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
ProfileViewManager #
:$ %
IProfileViewManager& 9
{ 
private 
readonly 

UNOContext #
_context$ ,
;, -
private 
readonly  
IProfileViewCallback -
	_callback. 7
;7 8
private 
ServiceResponse 
<  
ProfileData  +
>+ ,
	_response- 6
;6 7
public 
ProfileViewManager !
(! "
)" #
{ 	
_context 
= 
new 

UNOContext %
(% &
)& '
;' (
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D 
IProfileViewCallbackD X
>X Y
(Y Z
)Z [
;[ \
} 	
public 
void 
GetProfileData "
(" #
string# )
nickname* 2
)2 3
{ 	
try   
{!! 
var"" 
player"" 
="" 
_context"" %
.""% &
Player""& ,
."", -
FirstOrDefault""- ;
(""; <
p""< =
=>""> @
p""A B
.""B C
nickname""C K
==""L N
nickname""O W
)""W X
;""X Y
if## 
(## 
player## 
==## 
null## "
)##" #
{$$ 
	_response%% 
=%% 
new%%  #
ServiceResponse%%$ 3
<%%3 4
ProfileData%%4 ?
>%%? @
(%%@ A
false%%A F
,%%F G
MessageCode%%H S
.%%S T
PlayerNotFound%%T b
)%%b c
;%%c d
	_callback&& 
.&& 
ProfileDataReceived&& 1
(&&1 2
	_response&&2 ;
)&&; <
;&&< =
Logger'' 
.'' 
Log'' 
('' 
$"'' !
$str''! @
{''@ A
nickname''A I
}''I J
$str''J L
"''L M
)''M N
;''N O
return(( 
;(( 
})) 
var++ 
account++ 
=++ 
_context++ &
.++& '
Account++' .
.++. /
FirstOrDefault++/ =
(++= >
a++> ?
=>++@ B
a++C D
.++D E
Player_idPlayer++E T
==++U W
player++X ^
.++^ _
idPlayer++_ g
)++g h
;++h i
var,, 

statistics,, 
=,,  
_context,,! )
.,,) *
PlayerStatistics,,* :
.,,: ;
FirstOrDefault,,; I
(,,I J
s,,J K
=>,,L N
s,,O P
.,,P Q
Player_idPlayer,,Q `
==,,a c
player,,d j
.,,j k
idPlayer,,k s
),,s t
;,,t u
var-- 
socialNetworks-- "
=--# $
_context--% -
.--- .
SocialNetwork--. ;
... 
Where.. 
(.. 
sn.. 
=>..  
sn..! #
...# $
Player_idPlayer..$ 3
==..4 6
player..7 =
...= >
idPlayer..> F
)..F G
.// 
ToList// 
(// 
)// 
;// 
string11 
facebookUrl11 "
=11# $
socialNetworks11% 3
.113 4
FirstOrDefault114 B
(11B C
sn11C E
=>11F H
sn11I K
.11K L
tipoRedSocial11L Y
==11Z \
$str11] g
)11g h
?11h i
.11i j
linkRedSocial11j w
;11w x
string22 
instagramUrl22 #
=22$ %
socialNetworks22& 4
.224 5
FirstOrDefault225 C
(22C D
sn22D F
=>22G I
sn22J L
.22L M
tipoRedSocial22M Z
==22[ ]
$str22^ i
)22i j
?22j k
.22k l
linkRedSocial22l y
;22y z
string33 
	tikTokUrl33  
=33! "
socialNetworks33# 1
.331 2
FirstOrDefault332 @
(33@ A
sn33A C
=>33D F
sn33G I
.33I J
tipoRedSocial33J W
==33X Z
$str33[ c
)33c d
?33d e
.33e f
linkRedSocial33f s
;33s t
var55 
profileData55 
=55  !
new55" %
ProfileData55& 1
{66 
Nickname77 
=77 
player77 %
.77% &
nickname77& .
,77. /
FullName88 
=88 
player88 %
.88% &
fullName88& .
,88. /
Email:: 
=:: 
account:: #
?::# $
.::$ %
email::% *
,::* +
Password;; 
=;; 
PasswordHelper;; -
.;;- .
HashPassword;;. :
(;;: ;
account;;; B
?;;B C
.;;C D
password;;D L
);;L M
,;;M N
ExperiencePoints== $
===% &

statistics==' 1
?==1 2
.==2 3
globalPoints==3 ?
??==@ B
$num==C D
,==D E
MatchesPlayed>> !
=>>" #

statistics>>$ .
?>>. /
.>>/ 0
matchesPlayed>>0 =
??>>> @
$num>>A B
,>>B C
Wins?? 
=?? 

statistics?? %
???% &
.??& '
wins??' +
????, .
$num??/ 0
,??0 1
Losses@@ 
=@@ 

statistics@@ '
?@@' (
.@@( )
loses@@) .
??@@/ 1
$num@@2 3
,@@3 4
StreakAA 
=AA 

statisticsAA '
?AA' (
.AA( )
streakAA) /
??AA0 2
$numAA3 4
,AA4 5
	MaxStreakBB 
=BB 

statisticsBB  *
?BB* +
.BB+ ,
	maxStreakBB, 5
??BB6 8
$numBB9 :
,BB: ;
FacebookUrlDD 
=DD  !
facebookUrlDD" -
,DD- .
InstagramUrlEE  
=EE! "
instagramUrlEE# /
,EE/ 0
	TikTokUrlFF 
=FF 
	tikTokUrlFF  )
}GG 
;GG 
	_responseHH 
=HH 
newHH 
ServiceResponseHH  /
<HH/ 0
ProfileDataHH0 ;
>HH; <
(HH< =
trueHH= A
,HHA B
MessageCodeHHC N
.HHN O 
ProfileDataRetrievedHHO c
,HHc d
profileDataHHe p
)HHp q
;HHq r
	_callbackII 
.II 
ProfileDataReceivedII -
(II- .
	_responseII. 7
)II7 8
;II8 9
}JJ 
catchKK 
(KK "
CommunicationExceptionKK )
communicationExKK* 9
)KK9 :
{LL 
LoggerMM 
.MM 
LogMM 
(MM 
$"MM 
$strMM N
{MMN O
nicknameMMO W
}MMW X
$strMMX [
{MM[ \
communicationExMM\ k
.MMk l
MessageMMl s
}MMs t
"MMt u
)MMu v
;MMv w
	_responseNN 
=NN 
newNN 
ServiceResponseNN  /
<NN/ 0
ProfileDataNN0 ;
>NN; <
(NN< =
falseNN= B
,NNB C
MessageCodeNND O
.NNO P
ProfileFetchFailedNNP b
)NNb c
;NNc d
	_callbackOO 
.OO 
ProfileDataReceivedOO -
(OO- .
	_responseOO. 7
)OO7 8
;OO8 9
}PP 
catchQQ 
(QQ 
TimeoutExceptionQQ #
	timeoutExQQ$ -
)QQ- .
{RR 
LoggerSS 
.SS 
LogSS 
(SS 
$"SS 
$strSS Q
{SSQ R
nicknameSSR Z
}SSZ [
$strSS[ ^
{SS^ _
	timeoutExSS_ h
.SSh i
MessageSSi p
}SSp q
"SSq r
)SSr s
;SSs t
	_responseTT 
=TT 
newTT 
ServiceResponseTT  /
<TT/ 0
ProfileDataTT0 ;
>TT; <
(TT< =
falseTT= B
,TTB C
MessageCodeTTD O
.TTO P
TimeoutTTP W
)TTW X
;TTX Y
	_callbackUU 
.UU 
ProfileDataReceivedUU -
(UU- .
	_responseUU. 7
)UU7 8
;UU8 9
}VV 
catchWW 
(WW 
SqlExceptionWW 
dbExWW  $
)WW$ %
{XX 
LoggerYY 
.YY 
LogYY 
(YY 
$"YY 
$strYY O
{YYO P
nicknameYYP X
}YYX Y
$strYYY \
{YY\ ]
dbExYY] a
.YYa b
MessageYYb i
}YYi j
"YYj k
)YYk l
;YYl m
	_responseZZ 
=ZZ 
newZZ 
ServiceResponseZZ  /
<ZZ/ 0
ProfileDataZZ0 ;
>ZZ; <
(ZZ< =
falseZZ= B
,ZZB C
MessageCodeZZD O
.ZZO P
DatabaseErrorZZP ]
)ZZ] ^
;ZZ^ _
	_callback[[ 
.[[ 
ProfileDataReceived[[ -
([[- .
	_response[[. 7
)[[7 8
;[[8 9
}\\ 
catch]] 
(]] 
	Exception]] 
ex]] 
)]]  
{^^ 
Logger__ 
.__ 
Log__ 
(__ 
$"__ 
$str__ I
{__I J
nickname__J R
}__R S
$str__S V
{__V W
ex__W Y
.__Y Z
Message__Z a
}__a b
"__b c
)__c d
;__d e
	_response`` 
=`` 
new`` 
ServiceResponse``  /
<``/ 0
ProfileData``0 ;
>``; <
(``< =
false``= B
,``B C
MessageCode``D O
.``O P
GeneralServerError``P b
)``b c
;``c d
	_callbackaa 
.aa 
ProfileDataReceivedaa -
(aa- .
	_responseaa. 7
)aa7 8
;aa8 9
}bb 
}cc 	
}dd 
}ee Øw
TC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\ProfileEditManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
ProfileEditManager #
:$ %
IProfileEditManager& 9
{ 
private 
const 
string %
FacebookSocialNetworkType 6
=7 8
$str9 C
;C D
private 
const 
string &
InstagramSocialNetworkType 7
=8 9
$str: E
;E F
private 
const 
string #
TikTokSocialNetworkType 4
=5 6
$str7 ?
;? @
private 
readonly 

UNOContext #
_context$ ,
;, -
private 
readonly  
IProfileEditCallback -
	_callback. 7
;7 8
private 
ServiceResponse 
<  
ProfileData  +
>+ ,
	_response- 6
;6 7
public 
ProfileEditManager !
(! "
)" #
{ 	
_context 
= 
new 

UNOContext %
(% &
)& '
;' (
	_callback   
=   
OperationContext   (
.  ( )
Current  ) 0
.  0 1
GetCallbackChannel  1 C
<  C D 
IProfileEditCallback  D X
>  X Y
(  Y Z
)  Z [
;  [ \
}!! 	
public## 
void## 
UpdateProfileData## %
(##% &
ProfileData##& 1
data##2 6
)##6 7
{$$ 	
if%% 
(%% 
data%% 
==%% 
null%% 
)%% 
{&& 
	_response'' 
='' 
new'' 
ServiceResponse''  /
<''/ 0
ProfileData''0 ;
>''; <
(''< =
false''= B
,''B C
MessageCode''D O
.''O P
InvalidData''P [
)''[ \
;''\ ]
	_callback(( 
.(( !
ProfileUpdateResponse(( /
(((/ 0
	_response((0 9
)((9 :
;((: ;
return)) 
;)) 
}** 
using,, 
(,, 
var,, 
transaction,, "
=,,# $
_context,,% -
.,,- .
Database,,. 6
.,,6 7
BeginTransaction,,7 G
(,,G H
),,H I
),,I J
{-- 
try.. 
{// 
var00 
player00 
=00  
_context00! )
.00) *
Player00* 0
.000 1
FirstOrDefault001 ?
(00? @
p00@ A
=>00B D
p00E F
.00F G
nickname00G O
==00P R
data00S W
.00W X
Nickname00X `
)00` a
;00a b
if11 
(11 
player11 
==11 !
null11" &
)11& '
{22 
	_response33 !
=33" #
new33$ '
ServiceResponse33( 7
<337 8
ProfileData338 C
>33C D
(33D E
false33E J
,33J K
MessageCode33L W
.33W X
PlayerNotFound33X f
)33f g
;33g h
	_callback44 !
.44! "!
ProfileUpdateResponse44" 7
(447 8
	_response448 A
)44A B
;44B C
return55 
;55 
}66 
var88 
account88 
=88  !
_context88" *
.88* +
Account88+ 2
.882 3
FirstOrDefault883 A
(88A B
a88B C
=>88D F
a88G H
.88H I
Player_idPlayer88I X
==88Y [
player88\ b
.88b c
idPlayer88c k
)88k l
;88l m
if99 
(99 
account99 
==99  "
null99# '
)99' (
{:: 
	_response;; !
=;;" #
new;;$ '
ServiceResponse;;( 7
<;;7 8
ProfileData;;8 C
>;;C D
(;;D E
false;;E J
,;;J K
MessageCode;;L W
.;;W X
PlayerNotFound;;X f
);;f g
;;;g h
	_callback<< !
.<<! "!
ProfileUpdateResponse<<" 7
(<<7 8
	_response<<8 A
)<<A B
;<<B C
return== 
;== 
}>> 
player@@ 
.@@ 
fullName@@ #
=@@$ %
data@@& *
.@@* +
FullName@@+ 3
;@@3 4
accountAA 
.AA 
emailAA !
=AA" #
dataAA$ (
.AA( )
EmailAA) .
;AA. /
ifCC 
(CC 
!CC 
stringCC 
.CC  
IsNullOrWhiteSpaceCC  2
(CC2 3
dataCC3 7
.CC7 8
PasswordCC8 @
)CC@ A
)CCA B
{DD 
boolEE 
samePasswordEE )
=EE* +
PasswordHelperEE, :
.EE: ;
VerifyPasswordEE; I
(EEI J
dataEEJ N
.EEN O
PasswordEEO W
,EEW X
accountEEY `
.EE` a
passwordEEa i
)EEi j
;EEj k
ifFF 
(FF 
samePasswordFF (
)FF( )
{GG 
	_responseHH %
=HH& '
newHH( +
ServiceResponseHH, ;
<HH; <
ProfileDataHH< G
>HHG H
(HHH I
falseHHI N
,HHN O
MessageCodeHHP [
.HH[ \
SamePasswordHH\ h
)HHh i
;HHi j
	_callbackII %
.II% &!
ProfileUpdateResponseII& ;
(II; <
	_responseII< E
)IIE F
;IIF G
returnJJ "
;JJ" #
}KK 
accountLL 
.LL  
passwordLL  (
=LL) *
PasswordHelperLL+ 9
.LL9 :
HashPasswordLL: F
(LLF G
dataLLG K
.LLK L
PasswordLLL T
)LLT U
;LLU V
}MM 
varOO 
socialNetworksOO &
=OO' (
_contextOO) 1
.OO1 2
SocialNetworkOO2 ?
.PP 
WherePP 
(PP 
snPP !
=>PP" $
snPP% '
.PP' (
Player_idPlayerPP( 7
==PP8 :
playerPP; A
.PPA B
idPlayerPPB J
)PPJ K
.QQ 
ToListQQ 
(QQ  
)QQ  !
;QQ! "
UpdateOrAddNetworkSS &
(SS& '
playerSS' -
.SS- .
idPlayerSS. 6
,SS6 7
socialNetworksSS8 F
,SSF G
newSSH K
NetworkUpdateDataSSL ]
{TT 
TypeUU 
=UU %
FacebookSocialNetworkTypeUU 8
,UU8 9
UrlVV 
=VV 
dataVV "
.VV" #
FacebookUrlVV# .
}WW 
)WW 
;WW 
UpdateOrAddNetworkYY &
(YY& '
playerYY' -
.YY- .
idPlayerYY. 6
,YY6 7
socialNetworksYY8 F
,YYF G
newYYH K
NetworkUpdateDataYYL ]
{ZZ 
Type[[ 
=[[ &
InstagramSocialNetworkType[[ 9
,[[9 :
Url\\ 
=\\ 
data\\ "
.\\" #
InstagramUrl\\# /
}]] 
)]] 
;]] 
UpdateOrAddNetwork__ &
(__& '
player__' -
.__- .
idPlayer__. 6
,__6 7
socialNetworks__8 F
,__F G
new__H K
NetworkUpdateData__L ]
{`` 
Typeaa 
=aa #
TikTokSocialNetworkTypeaa 6
,aa6 7
Urlbb 
=bb 
databb "
.bb" #
	TikTokUrlbb# ,
}cc 
)cc 
;cc 
_contextee 
.ee 
SaveChangesee (
(ee( )
)ee) *
;ee* +
transactionff 
.ff  
Commitff  &
(ff& '
)ff' (
;ff( )
	_responsehh 
=hh 
newhh  #
ServiceResponsehh$ 3
<hh3 4
ProfileDatahh4 ?
>hh? @
(hh@ A
truehhA E
,hhE F
MessageCodehhG R
.hhR S
ProfileUpdatedhhS a
)hha b
;hhb c
	_callbackii 
.ii !
ProfileUpdateResponseii 3
(ii3 4
	_responseii4 =
)ii= >
;ii> ?
}jj 
catchkk 
(kk "
CommunicationExceptionkk -
communicationExkk. =
)kk= >
{ll 
Loggermm 
.mm 
Logmm 
(mm 
$"mm !
$strmm! `
{mm` a
datamma e
.mme f
Nicknamemmf n
}mmn o
$strmmo y
{mmy z
communicationEx	mmz â
.
mmâ ä
Message
mmä ë
}
mmë í
"
mmí ì
)
mmì î
;
mmî ï
transactionnn 
.nn  
Rollbacknn  (
(nn( )
)nn) *
;nn* +
}oo 
catchpp 
(pp 
TimeoutExceptionpp '
	timeoutExpp( 1
)pp1 2
{qq 
Loggerrr 
.rr 
Logrr 
(rr 
$"rr !
$strrr! R
{rrR S
datarrS W
.rrW X
NicknamerrX `
}rr` a
$strrra k
{rrk l
	timeoutExrrl u
.rru v
Messagerrv }
}rr} ~
"rr~ 
)	rr Ä
;
rrÄ Å
transactionss 
.ss  
Rollbackss  (
(ss( )
)ss) *
;ss* +
}tt 
catchuu 
(uu 
DbUpdateExceptionuu (

dbUpdateExuu) 3
)uu3 4
{vv 
	_responseww 
=ww 
newww  #
ServiceResponseww$ 3
<ww3 4
ProfileDataww4 ?
>ww? @
(ww@ A
falsewwA F
,wwF G
MessageCodewwH S
.wwS T
DatabaseErrorwwT a
)wwa b
;wwb c
Loggerxx 
.xx 
Logxx 
(xx 
$"xx !
$strxx! a
{xxa b
dataxxb f
.xxf g
Nicknamexxg o
}xxo p
$strxxp z
{xxz {

dbUpdateEx	xx{ Ö
.
xxÖ Ü
Message
xxÜ ç
}
xxç é
"
xxé è
)
xxè ê
;
xxê ë
transactionyy 
.yy  
Rollbackyy  (
(yy( )
)yy) *
;yy* +
	_callbackzz 
.zz !
ProfileUpdateResponsezz 3
(zz3 4
	_responsezz4 =
)zz= >
;zz> ?
}{{ 
catch|| 
(|| 
SqlException|| #
sqlEx||$ )
)||) *
{}} 
	_response~~ 
=~~ 
new~~  #
ServiceResponse~~$ 3
<~~3 4
ProfileData~~4 ?
>~~? @
(~~@ A
false~~A F
,~~F G
MessageCode~~H S
.~~S T
SqlError~~T \
)~~\ ]
;~~] ^
Logger 
. 
Log 
( 
$" !
$str! T
{T U
dataU Y
.Y Z
NicknameZ b
}b c
$strc m
{m n
sqlExn s
.s t
Messaget {
}{ |
"| }
)} ~
;~ 
transaction
ÄÄ 
.
ÄÄ  
Rollback
ÄÄ  (
(
ÄÄ( )
)
ÄÄ) *
;
ÄÄ* +
	_callback
ÅÅ 
.
ÅÅ #
ProfileUpdateResponse
ÅÅ 3
(
ÅÅ3 4
	_response
ÅÅ4 =
)
ÅÅ= >
;
ÅÅ> ?
}
ÇÇ 
catch
ÉÉ 
(
ÉÉ 
	Exception
ÉÉ  
ex
ÉÉ! #
)
ÉÉ# $
{
ÑÑ 
	_response
ÖÖ 
=
ÖÖ 
new
ÖÖ  #
ServiceResponse
ÖÖ$ 3
<
ÖÖ3 4
ProfileData
ÖÖ4 ?
>
ÖÖ? @
(
ÖÖ@ A
false
ÖÖA F
,
ÖÖF G
MessageCode
ÖÖH S
.
ÖÖS T!
ProfileUpdateFailed
ÖÖT g
)
ÖÖg h
;
ÖÖh i
Logger
ÜÜ 
.
ÜÜ 
Log
ÜÜ 
(
ÜÜ 
$"
ÜÜ !
$str
ÜÜ! [
{
ÜÜ[ \
data
ÜÜ\ `
.
ÜÜ` a
Nickname
ÜÜa i
}
ÜÜi j
$str
ÜÜj t
{
ÜÜt u
ex
ÜÜu w
.
ÜÜw x
Message
ÜÜx 
}ÜÜ Ä
"ÜÜÄ Å
)ÜÜÅ Ç
;ÜÜÇ É
transaction
áá 
.
áá  
Rollback
áá  (
(
áá( )
)
áá) *
;
áá* +
	_callback
àà 
.
àà #
ProfileUpdateResponse
àà 3
(
àà3 4
	_response
àà4 =
)
àà= >
;
àà> ?
}
ââ 
}
ää 
}
ãã 	
private
çç 
void
çç  
UpdateOrAddNetwork
çç '
(
çç' (
int
çç( +
playerId
çç, 4
,
çç4 5
List
çç6 :
<
çç: ;
SocialNetwork
çç; H
>
ççH I
existingNetworks
ççJ Z
,
ççZ [
NetworkUpdateData
çç\ m
data
ççn r
)
ççr s
{
éé 	
if
èè 
(
èè 
data
èè 
==
èè 
null
èè 
)
èè 
{
êê 
return
ëë 
;
ëë 
}
íí 
var
îî 
existing
îî 
=
îî 
existingNetworks
îî +
.
îî+ ,
FirstOrDefault
îî, :
(
îî: ;
sn
îî; =
=>
îî> @
sn
îîA C
.
îîC D
tipoRedSocial
îîD Q
==
îîR T
data
îîU Y
.
îîY Z
Type
îîZ ^
)
îî^ _
;
îî_ `
if
ïï 
(
ïï 
existing
ïï 
!=
ïï 
null
ïï  
)
ïï  !
{
ññ 
existing
óó 
.
óó 
linkRedSocial
óó &
=
óó' (
data
óó) -
.
óó- .
Url
óó. 1
;
óó1 2
}
òò 
else
ôô 
{
öö 
var
õõ 

newNetwork
õõ 
=
õõ  
new
õõ! $
SocialNetwork
õõ% 2
{
úú 
tipoRedSocial
ùù !
=
ùù" #
data
ùù$ (
.
ùù( )
Type
ùù) -
,
ùù- .
linkRedSocial
ûû !
=
ûû" #
data
ûû$ (
.
ûû( )
Url
ûû) ,
,
ûû, -
Player_idPlayer
üü #
=
üü$ %
playerId
üü& .
}
†† 
;
†† 
_context
°° 
.
°° 
SocialNetwork
°° &
.
°°& '
Add
°°' *
(
°°* +

newNetwork
°°+ 5
)
°°5 6
;
°°6 7
}
¢¢ 
}
££ 	
}
§§ 
}•• —7
RC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\PartyHostManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[		 
ServiceBehavior		 
(		 
InstanceContextMode		 (
=		) *
InstanceContextMode		+ >
.		> ?

PerSession		? I
,		I J
ConcurrencyMode		K Z
=		[ \
ConcurrencyMode		] l
.		l m
	Reentrant		m v
)		v w
]		w x
public

 

class

 
PartyHostManager

 !
:

" #
IPartyHostManager

$ 5
{ 
private 
readonly 
IPartyHostCallback +
	_callback, 5
;5 6
private 
static 
readonly 

Dictionary  *
<* +
int+ .
,. /
Lobby0 5
>5 6
ActiveParties7 D
=E F
newG J

DictionaryK U
<U V
intV Y
,Y Z
Lobby[ `
>` a
(a b
)b c
;c d
private 
static 
int 
_nextPartyId '
=( )
$num* +
;+ ,
public 
PartyHostManager 
(  
)  !
{ 	
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
IPartyHostCallbackD V
>V W
(W X
)X Y
;Y Z
} 	
public 
void 
CreateParty 
(  
string  &
hostNickname' 3
)3 4
{ 	
int 
partyId 
= 
_nextPartyId &
++& (
;( )
string 
joinCode 
= 
Guid "
." #
NewGuid# *
(* +
)+ ,
., -
ToString- 5
(5 6
)6 7
.7 8
	Substring8 A
(A B
$numB C
,C D
$numE F
)F G
.G H
ToUpperH O
(O P
)P Q
;Q R
var 
lobby 
= 
new 
Lobby !
{ 
PartyId 
= 
partyId !
,! "
JoinCode 
= 
joinCode #
,# $
HostNickname   
=   
hostNickname   +
,  + ,
Players!! 
=!! 
new!! 
List!! "
<!!" #
PlayerState!!# .
>!!. /
{!!0 1
new!!2 5
PlayerState!!6 A
(!!A B
hostNickname!!B N
)!!N O
}!!P Q
}"" 
;"" 
ActiveParties$$ 
[$$ 
partyId$$ !
]$$! "
=$$# $
lobby$$% *
;$$* +
Console&& 
.&& 
	WriteLine&& 
(&& 
$"&&  
$str&&  '
{&&' (
hostNickname&&( 4
}&&4 5
$str&&5 F
{&&F G
partyId&&G N
}&&N O
$str&&O Y
{&&Y Z
joinCode&&Z b
}&&b c
$str&&c d
"&&d e
)&&e f
;&&f g
	_callback'' 
.'' 
PartyCreated'' "
(''" #
partyId''# *
,''* +
joinCode'', 4
)''4 5
;''5 6
}(( 	
public** 
void** 

StartMatch** 
(** 
int** "
partyId**# *
,*** +
string**, 2
hostNickname**3 ?
)**? @
{++ 	
if,, 
(,, 
!,, 
ActiveParties,, 
.,, 
TryGetValue,, *
(,,* +
partyId,,+ 2
,,,2 3
out,,4 7
var,,8 ;
lobby,,< A
),,A B
),,B C
{-- 
Console.. 
... 
	WriteLine.. !
(..! "
$".." $
$str..$ @
{..@ A
partyId..A H
}..H I
"..I J
)..J K
;..K L
	_callback// 
.// 
PartyCancelled// (
(//( )
)//) *
;//* +
return00 
;00 
}11 
Console33 
.33 
	WriteLine33 
(33 
$"33  
$str33  '
{33' (
hostNickname33( 4
}334 5
$str335 H
{33H I
partyId33I P
}33P Q
"33Q R
)33R S
;33S T
	_callback44 
.44 
MatchStarted44 "
(44" #
)44# $
;44$ %
}55 	
public77 
void77 
CancelParty77 
(77  
int77  #
partyId77$ +
)77+ ,
{88 	
if99 
(99 
ActiveParties99 
.99 
Remove99 $
(99$ %
partyId99% ,
)99, -
)99- .
{:: 
Console;; 
.;; 
	WriteLine;; !
(;;! "
$";;" $
$str;;$ 3
{;;3 4
partyId;;4 ;
};;; <
$str;;< S
";;S T
);;T U
;;;U V
	_callback<< 
.<< 
PartyCancelled<< (
(<<( )
)<<) *
;<<* +
}== 
}>> 	
privateAA 
classAA 
LobbyAA 
{BB 	
publicCC 
intCC 
PartyIdCC 
{CC  
getCC! $
;CC$ %
setCC& )
;CC) *
}CC+ ,
publicDD 
stringDD 
JoinCodeDD "
{DD# $
getDD% (
;DD( )
setDD* -
;DD- .
}DD/ 0
publicEE 
stringEE 
HostNicknameEE &
{EE' (
getEE) ,
;EE, -
setEE. 1
;EE1 2
}EE3 4
publicFF 
ListFF 
<FF 
PlayerStateFF #
>FF# $
PlayersFF% ,
{FF- .
getFF/ 2
;FF2 3
setFF4 7
;FF7 8
}FF9 :
=FF; <
newFF= @
ListFFA E
<FFE F
PlayerStateFFF Q
>FFQ R
(FFR S
)FFS T
;FFT U
}HH 	
privateJJ 
classJJ 
PlayerStateJJ !
{KK 	
publicLL 
stringLL 
NicknameLL "
{LL# $
getLL% (
;LL( )
}LL* +
publicMM 
boolMM 
IsReadyMM 
{MM  !
getMM" %
;MM% &
setMM' *
;MM* +
}MM, -
publicOO 
PlayerStateOO 
(OO 
stringOO %
nicknameOO& .
)OO. /
{PP 
NicknameQQ 
=QQ 
nicknameQQ #
;QQ# $
IsReadyRR 
=RR 
falseRR 
;RR  
}SS 
}TT 	
}UU 
}VV ï"
TC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\PartyClientManager.cs
	namespace

 	
UnoLisServer


 
.

 
Services

 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
PartyClientManager #
:$ %
IPartyClientManager& 9
{ 
private 
readonly  
IPartyClientCallback -
	_callback. 7
;7 8
public 
PartyClientManager !
(! "
)" #
{ 	
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D 
IPartyClientCallbackD X
>X Y
(Y Z
)Z [
;[ \
} 	
public 
void 
	JoinParty 
( 
JoinPartyRequest .
request/ 6
)6 7
{ 	
Console 
. 
	WriteLine 
( 
$"  
$str  )
{) *
request* 1
.1 2
Nickname2 :
}: ;
$str; U
{U V
requestV ]
.] ^
JoinCode^ f
}f g
"g h
)h i
;i j
bool 
joinSuccess 
= 
true #
;# $
if 
( 
joinSuccess 
) 
{ 
int 
partyId 
= 
new !
Random" (
(( )
)) *
.* +
Next+ /
(/ 0
$num0 4
,4 5
$num6 :
): ;
;; <
	_callback 
. 
JoinedSuccessfully ,
(, -
partyId- 4
,4 5
$str6 @
)@ A
;A B
Console   
.   
	WriteLine   !
(  ! "
$"  " $
$str  $ -
{  - .
request  . 5
.  5 6
Nickname  6 >
}  > ?
$str  ? b
{  b c
partyId  c j
}  j k
"  k l
)  l m
;  m n
}!! 
else"" 
{## 
	_callback$$ 
.$$ 

JoinFailed$$ $
($$$ %
$str$$% N
)$$N O
;$$O P
}%% 
}&& 	
public(( 
void(( 

LeaveParty(( 
((( 
PartyActionData(( .
data((/ 3
)((3 4
{)) 	
Console** 
.** 
	WriteLine** 
(** 
$"**  
$str**  )
{**) *
data*** .
.**. /
Nickname**/ 7
}**7 8
$str**8 M
{**M N
data**N R
.**R S
PartyId**S Z
}**Z [
"**[ \
)**\ ]
;**] ^
	_callback++ 
.++ 

PlayerLeft++  
(++  !
data++! %
.++% &
Nickname++& .
)++. /
;++/ 0
},, 	
public.. 
void.. 
SetReadyStatus.. "
(.." #
PartyActionData..# 2
data..3 7
)..7 8
{// 	
Console00 
.00 
	WriteLine00 
(00 
$"00  
$str00  )
{00) *
data00* .
.00. /
Nickname00/ 7
}007 8
$str008 >
{00> ?
(00? @
data00@ D
.00D E
IsReady00E L
==00M O
true00P T
?00U V
$str00W ^
:00_ `
$str00a k
)00k l
}00l m
"00m n
)00n o
;00o p
	_callback11 
.11 $
PlayerReadyStatusChanged11 .
(11. /
data11/ 3
.113 4
Nickname114 <
,11< =
data11> B
.11B C
IsReady11C J
??11K M
false11N S
)11S T
;11T U
}22 	
}33 
}44 ˘*
OC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\LogoutManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
LogoutManager 
:  
ILogoutManager! /
{ 
private 
readonly 
ILogoutCallback (
	_callback) 2
;2 3
private 
ServiceResponse 
<  
object  &
>& '
	_response( 1
;1 2
public 
LogoutManager 
( 
) 
{ 	
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
ILogoutCallbackD S
>S T
(T U
)U V
;V W
} 	
public 
void 
Logout 
( 
string !
nickname" *
)* +
{ 	
try 
{ 
if 
( 
string 
. 
IsNullOrWhiteSpace -
(- .
nickname. 6
)6 7
)7 8
{ 
	_response 
= 
new  #
ServiceResponse$ 3
<3 4
object4 :
>: ;
(; <
false< A
,A B
MessageCodeC N
.N O
InvalidDataO Z
)Z [
;[ \
	_callback   
.   
LogoutResponse   ,
(  , -
	_response  - 6
)  6 7
;  7 8
return!! 
;!! 
}"" 
if$$ 
($$ 
!$$ 
SessionManager$$ #
.$$# $
IsOnline$$$ ,
($$, -
nickname$$- 5
)$$5 6
)$$6 7
{%% 
	_response&& 
=&& 
new&&  #
ServiceResponse&&$ 3
<&&3 4
object&&4 :
>&&: ;
(&&; <
false&&< A
,&&A B
MessageCode&&C N
.&&N O
UserNotConnected&&O _
)&&_ `
;&&` a
	_callback'' 
.'' 
LogoutResponse'' ,
('', -
	_response''- 6
)''6 7
;''7 8
Logger(( 
.(( 
Log(( 
((( 
$"(( !
$str((! B
{((B C
nickname((C K
}((K L
$str((L N
"((N O
)((O P
;((P Q
return)) 
;)) 
}** 
SessionManager,, 
.,, 
RemoveSession,, ,
(,,, -
nickname,,- 5
),,5 6
;,,6 7
	_response-- 
=-- 
new-- 
ServiceResponse--  /
<--/ 0
object--0 6
>--6 7
(--7 8
true--8 <
,--< =
MessageCode--> I
.--I J
LogoutSuccessful--J Z
)--Z [
;--[ \
	_callback.. 
... 
LogoutResponse.. (
(..( )
	_response..) 2
)..2 3
;..3 4
Logger// 
.// 
Log// 
(// 
$"// 
$str// &
{//& '
nickname//' /
}/// 0
$str//0 M
"//M N
)//N O
;//O P
}00 
catch11 
(11 "
CommunicationException11 )
communicationEx11* 9
)119 :
{22 
Logger33 
.33 
Log33 
(33 
$"33 
$str33 H
{33H I
nickname33I Q
}33Q R
$str33R \
{33\ ]
communicationEx33] l
.33l m
Message33m t
}33t u
"33u v
)33v w
;33w x
}44 
catch55 
(55 
TimeoutException55 #
	timeoutEx55$ -
)55- .
{66 
Logger77 
.77 
Log77 
(77 
$"77 
$str77 N
{77N O
nickname77O W
}77W X
$str77X b
{77b c
	timeoutEx77c l
.77l m
Message77m t
}77t u
"77u v
)77v w
;77w x
}88 
catch99 
(99 
	Exception99 
ex99 
)99  
{:: 
Logger;; 
.;; 
Log;; 
(;; 
$";; 
$str;; ;
{;;; <
nickname;;< D
};;D E
$str;;E H
{;;H I
ex;;I K
.;;K L
Message;;L S
};;S T
";;T U
);;U V
;;;V W
	_response<< 
=<< 
new<< 
ServiceResponse<<  /
<<</ 0
object<<0 6
><<6 7
(<<7 8
false<<8 =
,<<= >
MessageCode<<? J
.<<J K
LogoutInternalError<<K ^
)<<^ _
;<<_ `
	_callback== 
.== 
LogoutResponse== (
(==( )
	_response==) 2
)==2 3
;==3 4
}>> 
}?? 	
}@@ 
}AA ìW
NC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\LoginManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
LoginManager 
: 
ILoginManager  -
{ 
private 
readonly 

UNOContext #
_context$ ,
;, -
private 
readonly 
ILoginCallback '
	_callback( 1
;1 2
private 
ServiceResponse 
<  
object  &
>& '
	_response( 1
;1 2
public 
LoginManager 
( 
) 
{ 	
_context 
= 
new 

UNOContext %
(% &
)& '
;' (
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
ILoginCallbackD R
>R S
(S T
)T U
;U V
} 	
public 
void 
Login 
( 
AuthCredentials )
credentials* 5
)5 6
{ 	
try   
{!! 
if"" 
("" 
credentials"" 
==""  "
null""# '
||""( *
string""+ 1
.""1 2
IsNullOrWhiteSpace""2 D
(""D E
credentials""E P
.""P Q
Nickname""Q Y
)""Y Z
||""[ ]
string""^ d
.""d e
IsNullOrWhiteSpace""e w
(""w x
credentials	""x É
.
""É Ñ
Password
""Ñ å
)
""å ç
)
""ç é
{## 
	_response$$ 
=$$ 
new$$  #
ServiceResponse$$$ 3
<$$3 4
object$$4 :
>$$: ;
($$; <
false$$< A
,$$A B
MessageCode$$C N
.$$N O
EmptyFields$$O Z
)$$Z [
;$$[ \
	_callback%% 
.%% 
LoginResponse%% +
(%%+ ,
	_response%%, 5
)%%5 6
;%%6 7
return&& 
;&& 
}'' 
Logger(( 
.(( 
Log(( 
((( 
$"(( 
$str(( ?
{((? @
credentials((@ K
.((K L
Nickname((L T
}((T U
$str((U Y
"((Y Z
)((Z [
;(([ \
var** 
account** 
=** 
_context** &
.**& '
Account**' .
.**. /
FirstOrDefault**/ =
(**= >
a**> ?
=>**@ B
a**C D
.**D E
Player**E K
.**K L
nickname**L T
==**U W
credentials**X c
.**c d
Nickname**d l
)**l m
;**m n
if++ 
(++ 
account++ 
==++ 
null++ #
)++# $
{,, 
	_response-- 
=-- 
new--  #
ServiceResponse--$ 3
<--3 4
object--4 :
>--: ;
(--; <
false--< A
,--A B
MessageCode--C N
.--N O
PlayerNotFound--O ]
)--] ^
;--^ _
	_callback.. 
... 
LoginResponse.. +
(..+ ,
	_response.., 5
)..5 6
;..6 7
Logger// 
.// 
Log// 
(// 
$"// !
$str//! *
{//* +
credentials//+ 6
.//6 7
Nickname//7 ?
}//? @
$str//@ P
"//P Q
)//Q R
;//R S
return00 
;00 
}11 
bool33 
isPasswordValid33 $
=33% &
PasswordHelper33' 5
.335 6
VerifyPassword336 D
(33D E
credentials33E P
.33P Q
Password33Q Y
,33Y Z
account33[ b
.33b c
password33c k
)33k l
;33l m
if44 
(44 
!44 
isPasswordValid44 $
)44$ %
{55 
	_response66 
=66 
new66  #
ServiceResponse66$ 3
<663 4
object664 :
>66: ;
(66; <
false66< A
,66A B
MessageCode66C N
.66N O
InvalidCredentials66O a
)66a b
;66b c
	_callback77 
.77 
LoginResponse77 +
(77+ ,
	_response77, 5
)775 6
;776 7
Logger88 
.88 
Log88 
(88 
$"88 !
$str88! =
{88= >
credentials88> I
.88I J
Nickname88J R
}88R S
$str88S U
"88U V
)88V W
;88W X
return99 
;99 
}:: 
if<< 
(<< 
SessionManager<< "
.<<" #
IsOnline<<# +
(<<+ ,
account<<, 3
.<<3 4
Player<<4 :
.<<: ;
nickname<<; C
)<<C D
)<<D E
{== 
	_response>> 
=>> 
new>>  #
ServiceResponse>>$ 3
<>>3 4
object>>4 :
>>>: ;
(>>; <
false>>< A
,>>A B
MessageCode>>C N
.>>N O
DuplicateSession>>O _
)>>_ `
;>>` a
	_callback?? 
.?? 
LoginResponse?? +
(??+ ,
	_response??, 5
)??5 6
;??6 7
Logger@@ 
.@@ 
Log@@ 
(@@ 
$"@@ !
$str@@! B
{@@B C
credentials@@C N
.@@N O
Nickname@@O W
}@@W X
$str@@X Y
"@@Y Z
)@@Z [
;@@[ \
returnAA 
;AA 
}BB 
varDD 
sessionDD 
=DD 
OperationContextDD .
.DD. /
CurrentDD/ 6
.DD6 7
GetCallbackChannelDD7 I
<DDI J
ILoginCallbackDDJ X
>DDX Y
(DDY Z
)DDZ [
;DD[ \
SessionManagerEE 
.EE 

AddSessionEE )
(EE) *
accountEE* 1
.EE1 2
PlayerEE2 8
.EE8 9
nicknameEE9 A
,EEA B
sessionEEC J
)EEJ K
;EEK L
	_responseGG 
=GG 
newGG 
ServiceResponseGG  /
<GG/ 0
objectGG0 6
>GG6 7
(GG7 8
trueGG8 <
,GG< =
MessageCodeGG> I
.GGI J
LoginSuccessfulGGJ Y
)GGY Z
;GGZ [
	_callbackHH 
.HH 
LoginResponseHH '
(HH' (
	_responseHH( 1
)HH1 2
;HH2 3
LoggerII 
.II 
LogII 
(II 
$"II 
$strII &
{II& '
accountII' .
.II. /
PlayerII/ 5
.II5 6
nicknameII6 >
}II> ?
$strII? ]
"II] ^
)II^ _
;II_ `
}JJ 
catchKK 
(KK "
CommunicationExceptionKK )
communicationExKK* 9
)KK9 :
{LL 
	_responseMM 
=MM 
newMM 
ServiceResponseMM  /
<MM/ 0
objectMM0 6
>MM6 7
(MM7 8
falseMM8 =
,MM= >
MessageCodeMM? J
.MMJ K
ConnectionFailedMMK [
)MM[ \
;MM\ ]
LoggerNN 
.NN 
LogNN 
(NN 
$"NN 
$strNN 8
{NN8 9
credentialsNN9 D
?NND E
.NNE F
NicknameNNF N
??NNO Q
$strNNR _
}NN_ `
$strNN` j
{NNj k
communicationExNNk z
.NNz {
Message	NN{ Ç
}
NNÇ É
"
NNÉ Ñ
)
NNÑ Ö
;
NNÖ Ü
	_callbackOO 
.OO 
LoginResponseOO '
(OO' (
	_responseOO( 1
)OO1 2
;OO2 3
}PP 
catchQQ 
(QQ 
TimeoutExceptionQQ #
	timeoutExQQ$ -
)QQ- .
{RR 
	_responseSS 
=SS 
newSS 
ServiceResponseSS  /
<SS/ 0
objectSS0 6
>SS6 7
(SS7 8
falseSS8 =
,SS= >
MessageCodeSS? J
.SSJ K
TimeoutSSK R
)SSR S
;SSS T
LoggerTT 
.TT 
LogTT 
(TT 
$"TT 
$strTT <
{TT< =
credentialsTT= H
?TTH I
.TTI J
NicknameTTJ R
??TTS U
$strTTV c
}TTc d
$strTTd n
{TTn o
	timeoutExTTo x
.TTx y
Message	TTy Ä
}
TTÄ Å
"
TTÅ Ç
)
TTÇ É
;
TTÉ Ñ
	_callbackUU 
.UU 
LoginResponseUU '
(UU' (
	_responseUU( 1
)UU1 2
;UU2 3
}VV 
catchWW 
(WW 
SqlExceptionWW 
dbExWW  $
)WW$ %
{XX 
	_responseYY 
=YY 
newYY 
ServiceResponseYY  /
<YY/ 0
objectYY0 6
>YY6 7
(YY7 8
falseYY8 =
,YY= >
MessageCodeYY? J
.YYJ K
DatabaseErrorYYK X
)YYX Y
;YYY Z
LoggerZZ 
.ZZ 
LogZZ 
(ZZ 
$"ZZ 
$strZZ T
{ZZT U
credentialsZZU `
?ZZ` a
.ZZa b
NicknameZZb j
??ZZk m
$strZZn t
}ZZt u
$strZZu x
{ZZx y
dbExZZy }
}ZZ} ~
"ZZ~ 
)	ZZ Ä
;
ZZÄ Å
	_callback[[ 
.[[ 
LoginResponse[[ '
([[' (
	_response[[( 1
)[[1 2
;[[2 3
}\\ 
catch]] 
(]] 
	Exception]] 
ex]] 
)]]  
{^^ 
	_response__ 
=__ 
new__ 
ServiceResponse__  /
<__/ 0
object__0 6
>__6 7
(__7 8
false__8 =
,__= >
MessageCode__? J
.__J K
LoginInternalError__K ]
)__] ^
;__^ _
Logger`` 
.`` 
Log`` 
(`` 
$"`` 
$str`` 0
{``0 1
credentials``1 <
?``< =
.``= >
Nickname``> F
??``G I
$str``J P
}``P Q
$str``Q U
{``U V
ex``V X
}``X Y
"``Y Z
)``Z [
;``[ \
	_callbackaa 
.aa 
LoginResponseaa '
(aa' (
	_responseaa( 1
)aa1 2
;aa2 3
}bb 
}cc 	
}dd 
}ee ”
PC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\FriendsManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
FriendsManager 
:  !
IFriendsManager" 1
{ 
private 
readonly 
IFriendsCallback )
	_callback* 3
;3 4
public 
FriendsManager 
( 
) 
{ 	
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
IFriendsCallbackD T
>T U
(U V
)V W
;W X
} 	
public 
void 
GetFriendsList "
(" #
string# )
nickname* 2
)2 3
{ 	
var 
mockList 
= 
new 
List #
<# $

FriendData$ .
>. /
{ 
new 

FriendData 
{  
FriendNickname! /
=0 1
$str2 ;
,; <
StatusMessage= J
=K L
$strM W
}X Y
,Y Z
new   

FriendData   
{    
FriendNickname  ! /
=  0 1
$str  2 ;
,  ; <
StatusMessage  = J
=  K L
$str  M V
}  W X
}!! 
;!! 
	_callback## 
.## 
FriendsListReceived## )
(##) *
mockList##* 2
)##2 3
;##3 4
}$$ 	
public&& 
void&& 
SendFriendRequest&& %
(&&% &
FriendRequestData&&& 7
request&&8 ?
)&&? @
{'' 	
	_callback(( 
.(( 
FriendRequestResult(( )
((() *
true((* .
,((. /
$"((0 2
$str((2 F
{((F G
request((G N
.((N O
TargetNickname((O ]
}((] ^
$str((^ _
"((_ `
)((` a
;((a b
})) 	
public++ 
void++ 
AcceptFriendRequest++ '
(++' (
FriendRequestData++( 9
request++: A
)++A B
{,, 	
	_callback-- 
.-- 
FriendRequestResult-- )
(--) *
true--* .
,--. /
$"--0 2
$str--2 H
{--H I
request--I P
.--P Q
RequesterNickname--Q b
}--b c
$str--c d
"--d e
)--e f
;--f g
}.. 	
public00 
void00 
RemoveFriend00  
(00  !
FriendRequestData00! 2
request003 :
)00: ;
{11 	
	_callback22 
.22 
FriendRequestResult22 )
(22) *
true22* .
,22. /
$"220 2
$str222 8
{228 9
request229 @
.22@ A
TargetNickname22A O
}22O P
$str22P [
"22[ \
)22\ ]
;22] ^
}33 	
}44 
}55 ≠
QC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\GameplayManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
GameplayManager  
:! "
IGameplayManager# 3
{ 
private 
readonly 
IGameplayCallback *
	_callback+ 4
;4 5
public 
GameplayManager 
( 
)  
{ 	
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
IGameplayCallbackD U
>U V
(V W
)W X
;X Y
} 	
public 
void 
PlayCard 
( 
PlayCardData )
data* .
). /
{ 	
	_callback 
. 

CardPlayed  
(  !
data! %
.% &
Nickname& .
,. /
data0 4
.4 5

PlayedCard5 ?
)? @
;@ A
} 	
public 
void 
DrawCard 
( 
string #
nickname$ ,
), -
{ 	
	_callback 
. 
	CardDrawn 
(  
nickname  (
)( )
;) *
} 	
}!! 
}"" ∫!
UC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\LeaderboardsManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode (
=) *
InstanceContextMode+ >
.> ?

PerSession? I
,I J
ConcurrencyModeK Z
=[ \
ConcurrencyMode] l
.l m
	Reentrantm v
)v w
]w x
public 

class 
LeaderboardsManager $
:% & 
ILeaderboardsManager' ;
{ 
private 
readonly 

UNOContext #
_context$ ,
;, -
private 
readonly !
ILeaderboardsCallback .
	_callback/ 8
;8 9
public 
LeaderboardsManager "
(" #
)# $
{ 	
_context 
= 
new 

UNOContext %
(% &
)& '
;' (
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D!
ILeaderboardsCallbackD Y
>Y Z
(Z [
)[ \
;\ ]
} 	
public 
void 
GetLeaderboard "
(" #
)# $
{ 	
try 
{ 
var 
entries 
= 
_context &
.& '
Player' -
. 
Select 
( 
p 
=>  
new! $
LeaderboardEntry% 5
{   
Nickname!!  
=!!! "
p!!# $
.!!$ %
nickname!!% -
,!!- .
FullName""  
=""! "
p""# $
.""$ %
fullName""% -
}## 
)## 
.$$ 
ToList$$ 
($$ 
)$$ 
;$$ 
	_callback&& 
.&& 
LeaderboardReceived&& -
(&&- .
entries&&. 5
)&&5 6
;&&6 7
}'' 
catch(( 
((( 
	Exception(( 
ex(( 
)((  
{)) 
Console** 
.** 
	WriteLine** !
(**! "
$"**" $
$str**$ B
{**B C
ex**C E
.**E F
Message**F M
}**M N
"**N O
)**O P
;**P Q
}++ 
},, 	
public.. 
void.. 
GetPlayerRank.. !
(..! "
string.." (
nickname..) 1
)..1 2
{// 	
try00 
{11 
var22 
player22 
=22 
_context22 %
.22% &
Player22& ,
.22, -
FirstOrDefault22- ;
(22; <
p22< =
=>22> @
p22A B
.22B C
nickname22C K
==22L N
nickname22O W
)22W X
;22X Y
if33 
(33 
player33 
==33 
null33 "
)33" #
{44 
	_callback55 
.55 
PlayerRankReceived55 0
(550 1
null551 5
)555 6
;556 7
return66 
;66 
}77 
var99 
entry99 
=99 
new99 
LeaderboardEntry99  0
{:: 
Nickname;; 
=;; 
player;; %
.;;% &
nickname;;& .
,;;. /
FullName<< 
=<< 
player<< %
.<<% &
fullName<<& .
}== 
;== 
	_callback?? 
.?? 
PlayerRankReceived?? ,
(??, -
entry??- 2
)??2 3
;??3 4
}@@ 
catchAA 
(AA 
	ExceptionAA 
exAA 
)AA  
{BB 
ConsoleCC 
.CC 
	WriteLineCC !
(CC! "
$"CC" $
$strCC$ G
{CCG H
exCCH J
.CCJ K
MessageCCK R
}CCR S
"CCS T
)CCT U
;CCU V
}DD 
}EE 	
}FF 
}GG ø\
UC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\ConfirmationManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode 
= 
InstanceContextMode 1
.1 2

PerSession2 <
,< =
ConcurrencyMode 
= 
ConcurrencyMode )
.) *
	Reentrant* 3
)3 4
]4 5
public 

class 
ConfirmationManager $
:% & 
IConfirmationManager' ;
{ 
private 
readonly 

UNOContext #
_context$ ,
;, -
private 
readonly !
IConfirmationCallback .
	_callback/ 8
;8 9
private 
ServiceResponse 
<  
object  &
>& '
	_response( 1
;1 2
private 
readonly 
INotificationSender ,
_notificationSender- @
;@ A
private 
readonly #
IVerificationCodeHelper 0#
_verificationCodeHelper1 H
;H I
private 
readonly &
IPendingRegistrationHelper 3&
_pendingRegistrationHelper4 N
;N O
public 
ConfirmationManager "
(" #
)# $
{ 	
_context 
= 
new 

UNOContext %
(% &
)& '
;' (
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D!
IConfirmationCallbackD Y
>Y Z
(Z [
)[ \
;\ ]
_notificationSender!! 
=!!  !
NotificationSender!!" 4
.!!4 5
Instance!!5 =
;!!= >#
_verificationCodeHelper"" #
=""$ %"
VerificationCodeHelper""& <
.""< =
Instance""= E
;""E F&
_pendingRegistrationHelper## &
=##' (%
PendingRegistrationHelper##) B
.##B C
Instance##C K
;##K L
}$$ 	
public&& 
void&& 
ConfirmCode&& 
(&&  
string&&  &
email&&' ,
,&&, -
string&&. 4
code&&5 9
)&&9 :
{'' 	
Logger(( 
.(( 
Log(( 
((( 
$"(( 
$str(( ;
{((; <
email((< A
}((A B
$str((B F
"((F G
)((G H
;((H I
try)) 
{** 
var++ 
validationRequest++ %
=++& '
new++( +!
CodeValidationRequest++, A
{,, 

Identifier-- 
=--  
email--! &
,--& '
Code.. 
=.. 
code.. 
,..  
CodeType// 
=// 
(//  
int//  #
)//# $
CodeType//$ ,
.//, -
EmailVerification//- >
,//> ?
Consume00 
=00 
true00 "
}11 
;11 
bool22 
isCodeValid22  
=22! "#
_verificationCodeHelper22# :
.22: ;
ValidateCode22; G
(22G H
validationRequest22H Y
)22Y Z
;22Z [
if44 
(44 
!44 
isCodeValid44  
)44  !
{55 
Logger66 
.66 
Log66 
(66 
$"66 !
$str66! B
{66B C
email66C H
}66H I
$str66I K
"66K L
)66L M
;66M N
	_response77 
=77 
new77  #
ServiceResponse77$ 3
<773 4
object774 :
>77: ;
(77; <
false77< A
,77A B
MessageCode77C N
.77N O#
VerificationCodeInvalid77O f
)77f g
;77g h
	_callback88 
.88  
ConfirmationResponse88 2
(882 3
	_response883 <
)88< =
;88= >
return99 
;99 
}:: 
var<< 
pendingData<< 
=<<  !&
_pendingRegistrationHelper<<" <
.<<< =+
GetAndRemovePendingRegistration<<= \
(<<\ ]
email<<] b
)<<b c
;<<c d
if== 
(== 
pendingData== 
====  "
null==# '
)==' (
{>> 
Logger?? 
.?? 
Log?? 
(?? 
$"?? !
$str??! ^
{??^ _
email??_ d
}??d e
$str??e f
"??f g
)??g h
;??h i
	_response@@ 
=@@ 
new@@  #
ServiceResponse@@$ 3
<@@3 4
object@@4 :
>@@: ;
(@@; <
false@@< A
,@@A B
MessageCode@@C N
.@@N O 
RegistrationDataLost@@O c
)@@c d
;@@d e
	_callbackAA 
.AA  
ConfirmationResponseAA 2
(AA2 3
	_responseAA3 <
)AA< =
;AA= >
returnBB 
;BB 
}CC 
usingEE 
(EE 
varEE 
transactionEE &
=EE' (
_contextEE) 1
.EE1 2
DatabaseEE2 :
.EE: ;
BeginTransactionEE; K
(EEK L
)EEL M
)EEM N
{FF 
varGG 
	newPlayerGG !
=GG" #
newGG$ '
PlayerGG( .
{HH 
nicknameII  
=II! "
pendingDataII# .
.II. /
NicknameII/ 7
,II7 8
fullNameJJ  
=JJ! "
pendingDataJJ# .
.JJ. /
FullNameJJ/ 7
}KK 
;KK 
_contextLL 
.LL 
PlayerLL #
.LL# $
AddLL$ '
(LL' (
	newPlayerLL( 1
)LL1 2
;LL2 3
varNN 

newAccountNN "
=NN# $
newNN% (
AccountNN) 0
{OO 
emailPP 
=PP 
emailPP  %
,PP% &
passwordQQ  
=QQ! "
pendingDataQQ# .
.QQ. /
HashedPasswordQQ/ =
,QQ= >
PlayerRR 
=RR  
	newPlayerRR! *
}SS 
;SS 
_contextTT 
.TT 
AccountTT $
.TT$ %
AddTT% (
(TT( )

newAccountTT) 3
)TT3 4
;TT4 5
_contextVV 
.VV 
SaveChangesVV (
(VV( )
)VV) *
;VV* +
transactionWW 
.WW  
CommitWW  &
(WW& '
)WW' (
;WW( )
LoggerYY 
.YY 
LogYY 
(YY 
$"YY !
$strYY! (
{YY( )
emailYY) .
}YY. /
$strYY/ Q
"YYQ R
)YYR S
;YYS T
	_responseZZ 
=ZZ 
newZZ  #
ServiceResponseZZ$ 3
<ZZ3 4
objectZZ4 :
>ZZ: ;
(ZZ; <
trueZZ< @
,ZZ@ A
MessageCodeZZB M
.ZZM N"
RegistrationSuccessfulZZN d
)ZZd e
;ZZe f
	_callback[[ 
.[[  
ConfirmationResponse[[ 2
([[2 3
	_response[[3 <
)[[< =
;[[= >
}\\ 
}]] 
catch^^ 
(^^ 
	Exception^^ 
ex^^ 
)^^  
{__ 
Logger`` 
.`` 
Log`` 
(`` 
$"`` 
$str`` 2
{``2 3
email``3 8
}``8 9
$str``9 <
{``< =
ex``= ?
.``? @
Message``@ G
}``G H
"``H I
)``I J
;``J K
	_responseaa 
=aa 
newaa 
ServiceResponseaa  /
<aa/ 0
objectaa0 6
>aa6 7
(aa7 8
falseaa8 =
,aa= >
MessageCodeaa? J
.aaJ K
GeneralServerErroraaK ]
)aa] ^
;aa^ _
	_callbackbb 
.bb  
ConfirmationResponsebb .
(bb. /
	_responsebb/ 8
)bb8 9
;bb9 :
}cc 
}dd 	
publicff 
voidff "
ResendConfirmationCodeff *
(ff* +
stringff+ 1
emailff2 7
)ff7 8
{gg 	
Loggerhh 
.hh 
Loghh 
(hh 
$"hh 
$strhh =
{hh= >
emailhh> C
}hhC D
$strhhD G
"hhG H
)hhH I
;hhI J
tryii 
{jj 
ifkk 
(kk 
!kk #
_verificationCodeHelperkk ,
.kk, -
CanRequestCodekk- ;
(kk; <
emailkk< A
,kkA B
(kkC D
intkkD G
)kkG H
CodeTypekkH P
.kkP Q
EmailVerificationkkQ b
)kkb c
)kkc d
{ll 
	_responsemm 
=mm 
newmm  #
ServiceResponsemm$ 3
<mm3 4
objectmm4 :
>mm: ;
(mm; <
falsemm< A
,mmA B
MessageCodemmC N
.mmN O
RateLimitExceededmmO `
)mm` a
;mma b
Loggernn 
.nn 
Lognn 
(nn 
$"nn !
$strnn! G
{nnG H
emailnnH M
}nnM N
$strnnN e
"nne f
)nnf g
;nng h
	_callbackoo 
.oo 
ResendCodeResponseoo 0
(oo0 1
	_responseoo1 :
)oo: ;
;oo; <
returnpp 
;pp 
}qq 
varss 
newCodess 
=ss #
_verificationCodeHelperss 5
.ss5 6 
GenerateAndStoreCodess6 J
(ssJ K
emailssK P
,ssP Q
CodeTypessR Z
.ssZ [
EmailVerificationss[ l
)ssl m
;ssm n
_tt 
=tt 
_notificationSendertt '
.tt' (-
!SendAccountVerificationEmailAsynctt( I
(ttI J
emailttJ O
,ttO P
newCodettQ X
)ttX Y
;ttY Z
Loggervv 
.vv 
Logvv 
(vv 
$"vv 
$strvv 5
{vv5 6
emailvv6 ;
}vv; <
$strvv< >
"vv> ?
)vv? @
;vv@ A
	_responseww 
=ww 
newww 
ServiceResponseww  /
<ww/ 0
objectww0 6
>ww6 7
(ww7 8
trueww8 <
,ww< =
MessageCodeww> I
.wwI J"
VerificationCodeResentwwJ `
)ww` a
;wwa b
	_callbackxx 
.xx 
ResendCodeResponsexx ,
(xx, -
	_responsexx- 6
)xx6 7
;xx7 8
}yy 
catchzz 
(zz 
	Exceptionzz 
exzz 
)zz  
{{{ 
Logger|| 
.|| 
Log|| 
(|| 
$"|| 
$str|| =
{||= >
email||> C
}||C D
$str||D G
{||G H
ex||H J
.||J K
Message||K R
}||R S
"||S T
)||T U
;||U V
	_response}} 
=}} 
new}} 
ServiceResponse}}  /
<}}/ 0
object}}0 6
>}}6 7
(}}7 8
false}}8 =
,}}= >
MessageCode}}? J
.}}J K
GeneralServerError}}K ]
)}}] ^
;}}^ _
	_callback~~ 
.~~ 
ResendCodeResponse~~ ,
(~~, -
	_response~~- 6
)~~6 7
;~~7 8
} 
}
ÄÄ 	
}
ÅÅ 
}ÇÇ ú<
MC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\ChatManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{		 
[ 
ServiceBehavior 
( 
InstanceContextMode 
= 
InstanceContextMode 1
.1 2
Single2 8
,8 9
ConcurrencyMode 
= 
ConcurrencyMode )
.) *
	Reentrant* 3
)3 4
]4 5
public 

class 
ChatManager 
: 
IChatManager +
{ 
private 
readonly 

Dictionary #
<# $
string$ *
,* +
IChatCallback, 9
>9 :
_connectedClients; L
=M N
new 

Dictionary 
< 
string !
,! "
IChatCallback# 0
>0 1
(1 2
)2 3
;3 4
private 
readonly 
object 
	_syncLock  )
=* +
new, /
object0 6
(6 7
)7 8
;8 9
public 
void 
RegisterPlayer "
(" #
string# )
nickname* 2
)2 3
{ 	
var 
callback 
= 
OperationContext +
.+ ,
Current, 3
.3 4
GetCallbackChannel4 F
<F G
IChatCallbackG T
>T U
(U V
)V W
;W X
lock 
( 
	_syncLock 
) 
{ 
if 
( 
! 
_connectedClients &
.& '
ContainsKey' 2
(2 3
nickname3 ;
); <
)< =
{ 
_connectedClients   %
.  % &
Add  & )
(  ) *
nickname  * 2
,  2 3
callback  4 <
)  < =
;  = >
Console!! 
.!! 
	WriteLine!! %
(!!% &
$"!!& (
$str!!( *
{!!* +
nickname!!+ 3
}!!3 4
$str!!4 O
{!!O P
_connectedClients!!P a
.!!a b
Count!!b g
}!!g h
$str!!h z
"!!z {
)!!{ |
;!!| }
}"" 
else## 
{$$ 
_connectedClients%% %
[%%% &
nickname%%& .
]%%. /
=%%0 1
callback%%2 :
;%%: ;
Console&& 
.&& 
	WriteLine&& %
(&&% &
$"&&& (
$str&&( +
{&&+ ,
nickname&&, 4
}&&4 5
$str&&5 J
"&&J K
)&&K L
;&&L M
}'' 
}(( 
})) 	
public** 
void** 
SendMessage** 
(**  
ChatMessageData**  /
message**0 7
)**7 8
{++ 	
if,, 
(,, 
message,, 
==,, 
null,, 
),,  
return-- 
;-- 
Console// 
.// 
	WriteLine// 
(// 
$"//  
$str//  $
{//$ %
message//% ,
.//, -
Nickname//- 5
}//5 6
$str//6 8
{//8 9
message//9 @
.//@ A
Message//A H
}//H I
"//I J
)//J K
;//K L
List11 
<11 
string11 
>11 
disconnectedClients11 ,
=11- .
new11/ 2
List113 7
<117 8
string118 >
>11> ?
(11? @
)11@ A
;11A B
lock33 
(33 
	_syncLock33 
)33 
{44 
foreach55 
(55 
var55 
client55 #
in55$ &
_connectedClients55' 8
.558 9
ToList559 ?
(55? @
)55@ A
)55A B
{66 
try77 
{88 
client99 
.99 
Value99 $
.99$ %
MessageReceived99% 4
(994 5
message995 <
)99< =
;99= >
Console:: 
.::  
	WriteLine::  )
(::) *
$"::* ,
$str::, 8
{::8 9
client::9 ?
.::? @
Key::@ C
}::C D
"::D E
)::E F
;::F G
};; 
catch<< 
(<< "
CommunicationException<< 1
)<<1 2
{== 
Console>> 
.>>  
	WriteLine>>  )
(>>) *
$">>* ,
$str>>, 8
{>>8 9
client>>9 ?
.>>? @
Key>>@ C
}>>C D
$str>>D k
">>k l
)>>l m
;>>m n
disconnectedClients?? +
.??+ ,
Add??, /
(??/ 0
client??0 6
.??6 7
Key??7 :
)??: ;
;??; <
}@@ 
catchAA 
(AA 
	ExceptionAA $
exAA% '
)AA' (
{BB 
ConsoleCC 
.CC  
	WriteLineCC  )
(CC) *
$"CC* ,
$strCC, ?
{CC? @
clientCC@ F
.CCF G
KeyCCG J
}CCJ K
$strCCK M
{CCM N
exCCN P
.CCP Q
MessageCCQ X
}CCX Y
"CCY Z
)CCZ [
;CC[ \
}DD 
}EE 
foreachGG 
(GG 
varGG 
keyGG  
inGG! #
disconnectedClientsGG$ 7
)GG7 8
_connectedClientsHH %
.HH% &
RemoveHH& ,
(HH, -
keyHH- 0
)HH0 1
;HH1 2
}II 
}JJ 	
publicKK 
voidKK 
GetChatHistoryKK "
(KK" #
stringKK# )
	channelIdKK* 3
)KK3 4
{LL 	
varMM 
historyMM 
=MM 
newMM 
ListMM "
<MM" #
ChatMessageDataMM# 2
>MM2 3
{NN 
newOO 
ChatMessageDataOO #
{OO$ %
NicknameOO& .
=OO/ 0
$strOO1 8
,OO8 9
MessageOO: A
=OOB C
$strOOD L
}OOM N
,OON O
newPP 
ChatMessageDataPP #
{PP$ %
NicknamePP& .
=PP/ 0
$strPP1 6
,PP6 7
MessagePP8 ?
=PP@ A
$strPPB Y
}PPZ [
}QQ 
;QQ 
trySS 
{TT 
varUU 
callbackUU 
=UU 
OperationContextUU /
.UU/ 0
CurrentUU0 7
.UU7 8
GetCallbackChannelUU8 J
<UUJ K
IChatCallbackUUK X
>UUX Y
(UUY Z
)UUZ [
;UU[ \
callbackVV 
.VV 
ChatHistoryReceivedVV ,
(VV, -
historyVV- 4
.VV4 5
ToArrayVV5 <
(VV< =
)VV= >
)VV> ?
;VV? @
ConsoleWW 
.WW 
	WriteLineWW !
(WW! "
$"WW" $
$strWW$ :
{WW: ;
historyWW; B
.WWB C
CountWWC H
}WWH I
$strWWI T
"WWT U
)WWU V
;WWV W
}XX 
catchYY 
(YY 
	ExceptionYY 
exYY 
)YY  
{ZZ 
Console[[ 
.[[ 
	WriteLine[[ !
([[! "
$"[[" $
$str[[$ B
{[[B C
ex[[C E
.[[E F
Message[[F M
}[[M N
"[[N O
)[[O P
;[[P Q
}\\ 
}]] 	
}^^ 
}__ 