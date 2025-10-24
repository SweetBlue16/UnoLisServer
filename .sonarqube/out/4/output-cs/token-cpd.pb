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
}"" éX
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
;1 2
public 
RegisterManager 
( 
)  
{ 	
_context 
= 
new 

UNOContext %
(% &
)& '
;' (
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
IRegisterCallbackD U
>U V
(V W
)W X
;X Y
} 	
public   
void   
Register   
(   
RegistrationData   -
data  . 2
)  2 3
{!! 	
if"" 
("" 
data"" 
=="" 
null"" 
)"" 
{## 
	_response$$ 
=$$ 
new$$ 
ServiceResponse$$  /
<$$/ 0
object$$0 6
>$$6 7
($$7 8
false$$8 =
,$$= >
MessageCode$$? J
.$$J K
InvalidData$$K V
)$$V W
;$$W X
	_callback%% 
.%% 
RegisterResponse%% *
(%%* +
	_response%%+ 4
)%%4 5
;%%5 6
return&& 
;&& 
}'' 
using)) 
()) 
var)) 
transaction)) "
=))# $
_context))% -
.))- .
Database)). 6
.))6 7
BeginTransaction))7 G
())G H
)))H I
)))I J
{** 
try++ 
{,, 
Logger-- 
.-- 
Log-- 
(-- 
$"-- !
$str--! B
{--B C
data--C G
.--G H
Nickname--H P
}--P Q
$str--Q U
"--U V
)--V W
;--W X
bool// 
existsPlayer// %
=//& '
_context//( 0
.//0 1
Player//1 7
.//7 8
Any//8 ;
(//; <
p//< =
=>//> @
p//A B
.//B C
nickname//C K
==//L N
data//O S
.//S T
Nickname//T \
)//\ ]
;//] ^
if00 
(00 
existsPlayer00 $
)00$ %
{11 
	_response22 !
=22" #
new22$ '
ServiceResponse22( 7
<227 8
object228 >
>22> ?
(22? @
false22@ E
,22E F
MessageCode22G R
.22R S 
NicknameAlreadyTaken22S g
)22g h
;22h i
	_callback33 !
.33! "
RegisterResponse33" 2
(332 3
	_response333 <
)33< =
;33= >
Logger44 
.44 
Log44 "
(44" #
$"44# %
$str44% 2
{442 3
data443 7
.447 8
Nickname448 @
}44@ A
$str44A V
"44V W
)44W X
;44X Y
return55 
;55 
}66 
bool88 
existsAccount88 &
=88' (
_context88) 1
.881 2
Account882 9
.889 :
Any88: =
(88= >
a88> ?
=>88@ B
a88C D
.88D E
email88E J
==88K M
data88N R
.88R S
Email88S X
)88X Y
;88Y Z
if99 
(99 
existsAccount99 %
)99% &
{:: 
	_response;; !
=;;" #
new;;$ '
ServiceResponse;;( 7
<;;7 8
object;;8 >
>;;> ?
(;;? @
false;;@ E
,;;E F
MessageCode;;G R
.;;R S"
EmailAlreadyRegistered;;S i
);;i j
;;;j k
	_callback<< !
.<<! "
RegisterResponse<<" 2
(<<2 3
	_response<<3 <
)<<< =
;<<= >
Logger== 
.== 
Log== "
(==" #
$"==# %
$str==% /
{==/ 0
data==0 4
.==4 5
Email==5 :
}==: ;
$str==; P
"==P Q
)==Q R
;==R S
return>> 
;>> 
}?? 
varAA 
	newPlayerAA !
=AA" #
newAA$ '
PlayerAA( .
{BB 
nicknameCC  
=CC! "
dataCC# '
.CC' (
NicknameCC( 0
,CC0 1
fullNameDD  
=DD! "
dataDD# '
.DD' (
FullNameDD( 0
}EE 
;EE 
_contextFF 
.FF 
PlayerFF #
.FF# $
AddFF$ '
(FF' (
	newPlayerFF( 1
)FF1 2
;FF2 3
varHH 

newAccountHH "
=HH# $
newHH% (
AccountHH) 0
{II 
emailJJ 
=JJ 
dataJJ  $
.JJ$ %
EmailJJ% *
,JJ* +
passwordKK  
=KK! "
PasswordHelperKK# 1
.KK1 2
HashPasswordKK2 >
(KK> ?
dataKK? C
.KKC D
PasswordKKD L
)KKL M
,KKM N
PlayerLL 
=LL  
	newPlayerLL! *
}MM 
;MM 
_contextNN 
.NN 
AccountNN $
.NN$ %
AddNN% (
(NN( )

newAccountNN) 3
)NN3 4
;NN4 5
_contextPP 
.PP 
SaveChangesPP (
(PP( )
)PP) *
;PP* +
transactionQQ 
.QQ  
CommitQQ  &
(QQ& '
)QQ' (
;QQ( )
	_responseSS 
=SS 
newSS  #
ServiceResponseSS$ 3
<SS3 4
objectSS4 :
>SS: ;
(SS; <
trueSS< @
,SS@ A
MessageCodeSSB M
.SSM N"
RegistrationSuccessfulSSN d
)SSd e
;SSe f
	_callbackTT 
.TT 
RegisterResponseTT .
(TT. /
	_responseTT/ 8
)TT8 9
;TT9 :
LoggerUU 
.UU 
LogUU 
(UU 
$"UU !
$strUU! 7
{UU7 8
dataUU8 <
.UU< =
NicknameUU= E
}UUE F
$strUUF G
"UUG H
)UUH I
;UUI J
}VV 
catchWW 
(WW "
CommunicationExceptionWW -
communicationExWW. =
)WW= >
{XX 
transactionYY 
.YY  
RollbackYY  (
(YY( )
)YY) *
;YY* +
LoggerZZ 
.ZZ 
LogZZ 
(ZZ 
$"ZZ !
$strZZ! C
{ZZC D
dataZZD H
.ZZH I
EmailZZI N
}ZZN O
$strZZO R
{ZZR S
communicationExZZS b
.ZZb c
MessageZZc j
}ZZj k
"ZZk l
)ZZl m
;ZZm n
}[[ 
catch\\ 
(\\ 
TimeoutException\\ '
	timeoutEx\\( 1
)\\1 2
{]] 
transaction^^ 
.^^  
Rollback^^  (
(^^( )
)^^) *
;^^* +
Logger__ 
.__ 
Log__ 
(__ 
$"__ !
$str__! 5
{__5 6
data__6 :
.__: ;
Email__; @
}__@ A
$str__A D
{__D E
	timeoutEx__E N
.__N O
Message__O V
}__V W
"__W X
)__X Y
;__Y Z
}`` 
catchaa 
(aa 
DbUpdateExceptionaa (

dbUpdateExaa) 3
)aa3 4
{bb 
	_responsecc 
=cc 
newcc  #
ServiceResponsecc$ 3
<cc3 4
objectcc4 :
>cc: ;
(cc; <
falsecc< A
,ccA B
MessageCodeccC N
.ccN O
DatabaseErrorccO \
)cc\ ]
;cc] ^
transactiondd 
.dd  
Rollbackdd  (
(dd( )
)dd) *
;dd* +
Loggeree 
.ee 
Logee 
(ee 
$"ee !
$stree! D
{eeD E
dataeeE I
.eeI J
EmaileeJ O
}eeO P
$streeP S
{eeS T

dbUpdateExeeT ^
.ee^ _
Messageee_ f
}eef g
"eeg h
)eeh i
;eei j
	_callbackff 
.ff 
RegisterResponseff .
(ff. /
	_responseff/ 8
)ff8 9
;ff9 :
}gg 
catchhh 
(hh 
SqlExceptionhh #
dbExhh$ (
)hh( )
{ii 
transactionjj 
.jj  
Rollbackjj  (
(jj( )
)jj) *
;jj* +
	_responsekk 
=kk 
newkk  #
ServiceResponsekk$ 3
<kk3 4
objectkk4 :
>kk: ;
(kk; <
falsekk< A
,kkA B
MessageCodekkC N
.kkN O
SqlErrorkkO W
)kkW X
;kkX Y
Loggerll 
.ll 
Logll 
(ll 
$"ll !
$strll! 7
{ll7 8
datall8 <
.ll< =
Emailll= B
}llB C
$strllC F
{llF G
dbExllG K
.llK L
MessagellL S
}llS T
"llT U
)llU V
;llV W
	_callbackmm 
.mm 
RegisterResponsemm .
(mm. /
	_responsemm/ 8
)mm8 9
;mm9 :
}nn 
catchoo 
(oo 
	Exceptionoo  
exoo! #
)oo# $
{pp 
	_responseqq 
=qq 
newqq  #
ServiceResponseqq$ 3
<qq3 4
objectqq4 :
>qq: ;
(qq; <
falseqq< A
,qqA B
MessageCodeqqC N
.qqN O
GeneralServerErrorqqO a
)qqa b
;qqb c
transactionrr 
.rr  
Rollbackrr  (
(rr( )
)rr) *
;rr* +
Loggerss 
.ss 
Logss 
(ss 
$"ss !
$strss! 3
{ss3 4
datass4 8
.ss8 9
Emailss9 >
}ss> ?
$strss? B
{ssB C
exssC E
.ssE F
MessagessF M
}ssM N
"ssN O
)ssO P
;ssP Q
	_callbacktt 
.tt 
RegisterResponsett .
(tt. /
	_responsett/ 8
)tt8 9
;tt9 :
}uu 
}vv 
}ww 	
}xx 
}yy Ñ
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
]!!) *àU
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
<  
object  &
>& '
	_response( 1
;1 2
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
<%%3 4
object%%4 :
>%%: ;
(%%; <
false%%< A
,%%A B
MessageCode%%C N
.%%N O
PlayerNotFound%%O ]
)%%] ^
;%%^ _
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
<HH/ 0
objectHH0 6
>HH6 7
(HH7 8
trueHH8 <
,HH< =
MessageCodeHH> I
.HHI J 
ProfileDataRetrievedHHJ ^
,HH^ _
profileDataHH` k
)HHk l
;HHl m
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
<NN/ 0
objectNN0 6
>NN6 7
(NN7 8
falseNN8 =
,NN= >
MessageCodeNN? J
.NNJ K
ProfileFetchFailedNNK ]
)NN] ^
;NN^ _
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
<TT/ 0
objectTT0 6
>TT6 7
(TT7 8
falseTT8 =
,TT= >
MessageCodeTT? J
.TTJ K
TimeoutTTK R
)TTR S
;TTS T
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
<ZZ/ 0
objectZZ0 6
>ZZ6 7
(ZZ7 8
falseZZ8 =
,ZZ= >
MessageCodeZZ? J
.ZZJ K
DatabaseErrorZZK X
)ZZX Y
;ZZY Z
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
<``/ 0
object``0 6
>``6 7
(``7 8
false``8 =
,``= >
MessageCode``? J
.``J K
GeneralServerError``K ]
)``] ^
;``^ _
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
}ee Çw
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
<  
object  &
>& '
	_response( 1
;1 2
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
<''/ 0
object''0 6
>''6 7
(''7 8
false''8 =
,''= >
MessageCode''? J
.''J K
InvalidData''K V
)''V W
;''W X
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
<337 8
object338 >
>33> ?
(33? @
false33@ E
,33E F
MessageCode33G R
.33R S
PlayerNotFound33S a
)33a b
;33b c
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
<;;7 8
object;;8 >
>;;> ?
(;;? @
false;;@ E
,;;E F
MessageCode;;G R
.;;R S
PlayerNotFound;;S a
);;a b
;;;b c
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
<HH; <
objectHH< B
>HHB C
(HHC D
falseHHD I
,HHI J
MessageCodeHHK V
.HHV W
SamePasswordHHW c
)HHc d
;HHd e
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
<hh3 4
objecthh4 :
>hh: ;
(hh; <
truehh< @
,hh@ A
MessageCodehhB M
.hhM N
ProfileUpdatedhhN \
)hh\ ]
;hh] ^
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
<ww3 4
objectww4 :
>ww: ;
(ww; <
falseww< A
,wwA B
MessageCodewwC N
.wwN O
DatabaseErrorwwO \
)ww\ ]
;ww] ^
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
<~~3 4
object~~4 :
>~~: ;
(~~; <
false~~< A
,~~A B
MessageCode~~C N
.~~N O
SqlError~~O W
)~~W X
;~~X Y
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
ÖÖ3 4
object
ÖÖ4 :
>
ÖÖ: ;
(
ÖÖ; <
false
ÖÖ< A
,
ÖÖA B
MessageCode
ÖÖC N
.
ÖÖN O!
ProfileUpdateFailed
ÖÖO b
)
ÖÖb c
;
ÖÖc d
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
}44 “
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
} ˘*
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
}ee ∫!
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
}GG ≠
QC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\GameplayManager.cs
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
class 
GameplayManager  
:! "
IGameplayManager# 3
{ 
private 
readonly 
IGameplayCallback *
	_callback+ 4
;4 5
public 
GameplayManager 
( 
)  
{ 	
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
IGameplayCallbackD U
>U V
(V W
)W X
;X Y
} 	
public 
void 
PlayCard 
( 
PlayCardData )
data* .
). /
{ 	
	_callback 
. 

CardPlayed  
(  !
data! %
.% &
Nickname& .
,. /
data0 4
.4 5

PlayedCard5 ?
)? @
;@ A
} 	
public 
void 
DrawCard 
( 
string #
nickname$ ,
), -
{ 	
	_callback 
. 
	CardDrawn 
(  
nickname  (
)( )
;) *
}   	
}"" 
}## ”
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
}55 €
UC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\ConfirmationManager.cs
	namespace 	
UnoLisServer
 
. 
Services 
{ 
[ 
ServiceBehavior 
( 
InstanceContextMode 
= 
InstanceContextMode 1
.1 2

PerSession2 <
,< =
ConcurrencyMode 
= 
ConcurrencyMode )
.) *
	Reentrant* 3
)3 4
]4 5
public 

class 
ConfirmationManager $
:% & 
IConfirmationManager' ;
{ 
private 
readonly 

UNOContext #
_context$ ,
;, -
private 
readonly !
IConfirmationCallback .
	_callback/ 8
;8 9
public 
ConfirmationManager "
(" #
)# $
{ 	
_context 
= 
new 

UNOContext %
(% &
)& '
;' (
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D!
IConfirmationCallbackD Y
>Y Z
(Z [
)[ \
;\ ]
} 	
public 
void 
ConfirmCode 
(  
string  &
email' ,
,, -
string. 4
code5 9
)9 :
{ 	
Logger 
. 
Log 
( 
$" 
$str 1
{1 2
email2 7
}7 8
$str8 :
{: ;
code; ?
}? @
"@ A
)A B
;B C
	_callback 
.  
ConfirmationResponse *
(* +
true+ /
)/ 0
;0 1
}   	
}!! 
}"" Ç
MC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Services\ChatManager.cs
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
class 
ChatManager 
: 
IChatManager +
{ 
private 
readonly 
IChatCallback &
	_callback' 0
;0 1
public 
ChatManager 
( 
) 
{ 	
	_callback 
= 
OperationContext (
.( )
Current) 0
.0 1
GetCallbackChannel1 C
<C D
IChatCallbackD Q
>Q R
(R S
)S T
;T U
} 	
public 
void 
SendMessage 
(  
ChatMessageData  /
message0 7
)7 8
{ 	
Console 
. 
	WriteLine 
( 
$"  
$str  !
{! "
message" )
.) *
Nickname* 2
}2 3
$str3 5
{5 6
message6 =
.= >
Message> E
}E F
"F G
)G H
;H I
	_callback 
. 
MessageReceived %
(% &
message& -
)- .
;. /
} 	
public 
void 
GetChatHistory "
(" #
string# )
	channelId* 3
)3 4
{ 	
var 
history 
= 
new 
List "
<" #
ChatMessageData# 2
>2 3
{ 
new   
ChatMessageData   #
{  $ %
Nickname  & .
=  / 0
$str  1 8
,  8 9
Message  : A
=  B C
$str  D L
}  M N
,  N O
new!! 
ChatMessageData!! #
{!!$ %
Nickname!!& .
=!!/ 0
$str!!1 6
,!!6 7
Message!!8 ?
=!!@ A
$str!!B Y
}!!Z [
}"" 
;"" 
	_callback$$ 
.$$ 
ChatHistoryReceived$$ )
($$) *
history$$* 1
)$$1 2
;$$2 3
}%% 	
}&& 
}'' 