Ç
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
]!!) *Ù
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
}   ◊
SC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Models\ResponseInfo.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
Models $
{		 
public

 

class

 
ResponseInfo

 
<

 
T

 
>

  
{ 
public 
MessageCode 
MessageCode &
{' (
get) ,
;, -
set. 1
;1 2
}3 4
public 
bool 
Success 
{ 
get !
;! "
set# &
;& '
}( )
public 
string 

LogMessage  
{! "
get# &
;& '
set( +
;+ ,
}- .
public 
T 
Data 
{ 
get 
; 
set  
;  !
}" #
public 
ResponseInfo 
( 
MessageCode '
messageCode( 3
,3 4
bool5 9
success: A
=B C
falseD I
,I J
stringK Q

logMessageR \
=] ^
$str_ a
,a b
Tc d
datae i
=j k
defaultl s
(s t
Tt u
)u v
)v w
{ 	
Success 
= 
success 
; 
MessageCode 
= 
messageCode %
;% &

LogMessage 
= 

logMessage #
;# $
Data 
= 
data 
; 
} 	
} 
} ´
ZC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Models\PendingRegistration.cs
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
class		 
PendingRegistration		 $
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
string 
HashedPassword $
{% &
get' *
;* +
set, /
;/ 0
}1 2
} 
} º
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
} Ì
\C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Models\CodeValidationRequest.cs
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
class		 !
CodeValidationRequest		 &
{

 
public 
string 

Identifier  
{! "
get# &
;& '
set( +
;+ ,
}- .
public 
string 
Code 
{ 
get  
;  !
set" %
;% &
}' (
public 
int 
CodeType 
{ 
get !
;! "
set# &
;& '
}( )
public 
bool 
Consume 
{ 
get !
;! "
set# &
;& '
}( )
=* +
true, 0
;0 1
} 
} ùB
^C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Helpers\VerificationCodeHelper.cs
	namespace		 	
UnoLisServer		
 
.		 
Common		 
.		 
Helpers		 %
{

 
public 

	interface #
IVerificationCodeHelper ,
{ 
string  
GenerateAndStoreCode #
(# $
string$ *

identifier+ 5
,5 6
CodeType7 ?
type@ D
)D E
;E F
bool 
ValidateCode 
( !
CodeValidationRequest /
request0 7
)7 8
;8 9
bool 
CanRequestCode 
( 
string "

identifier# -
,- .
CodeType/ 7
type8 <
)< =
;= >
} 
public 

class "
VerificationCodeHelper '
:( )#
IVerificationCodeHelper* A
{ 
private 
static 
readonly 
Lazy  $
<$ %"
VerificationCodeHelper% ;
>; <
	_instance= F
=G H
new 
Lazy 
< "
VerificationCodeHelper +
>+ ,
(, -
(- .
). /
=>0 2
new3 6"
VerificationCodeHelper7 M
(M N
)N O
)O P
;P Q
private 
readonly 
object 
_lockObject  +
=, -
new. 1
object2 8
(8 9
)9 :
;: ;
private 
readonly 

Dictionary #
<# $
string$ *
,* +
CodeInfo, 4
>4 5
_codeStorage6 B
=C D
new 

Dictionary 
< 
string !
,! "
CodeInfo# +
>+ ,
(, -
)- .
;. /
private 
readonly 
Random 
_random  '
=( )
new* -
Random. 4
(4 5
)5 6
;6 7
public 
static #
IVerificationCodeHelper -
Instance. 6
=>7 9
	_instance: C
.C D
ValueD I
;I J
private "
VerificationCodeHelper &
(& '
)' (
{) *
}+ ,
sealed 
class 
CodeInfo 
{   	
public!! 
string!! 
Code!! 
{!!  
get!!! $
;!!$ %
set!!& )
;!!) *
}!!+ ,
public"" 
DateTime"" 
CreationTime"" (
{"") *
get""+ .
;"". /
set""0 3
;""3 4
}""5 6
public## 
DateTime## 
ExpirationTime## *
{##+ ,
get##- 0
;##0 1
set##2 5
;##5 6
}##7 8
public$$ 
CodeType$$ 
Type$$  
{$$! "
get$$# &
;$$& '
set$$( +
;$$+ ,
}$$- .
}%% 	
public'' 
bool'' 
CanRequestCode'' "
(''" #
string''# )

identifier''* 4
,''4 5
CodeType''6 >
type''? C
)''C D
{(( 	
lock)) 
()) 
_lockObject)) 
))) 
{** 
if++ 
(++ 
_codeStorage++  
.++  !
TryGetValue++! ,
(++, -
GetKey++- 3
(++3 4

identifier++4 >
,++> ?
type++@ D
)++D E
,++E F
out++G J
CodeInfo++K S
existingCode++T `
)++` a
&&++b d
existingCode,,  
.,,  !
Type,,! %
==,,& (
type,,) -
),,- .
{-- 
return.. 
DateTime.. #
...# $
UtcNow..$ *
...* +
Subtract..+ 3
(..3 4
existingCode..4 @
...@ A
CreationTime..A M
)..M N
...N O
TotalSeconds..O [
>=..\ ^
$num.._ a
;..a b
}// 
return00 
true00 
;00 
}11 
}22 	
public44 
string44  
GenerateAndStoreCode44 *
(44* +
string44+ 1

identifier442 <
,44< =
CodeType44> F
type44G K
)44K L
{55 	
var66 
key66 
=66 
GetKey66 
(66 

identifier66 '
,66' (
type66) -
)66- .
;66. /
lock77 
(77 
_lockObject77 
)77 
{88 
var99 
code99 
=99 
_random99 "
.99" #
Next99# '
(99' (
$num99( .
,99. /
$num990 6
)996 7
.997 8
ToString998 @
(99@ A
$str99A E
)99E F
;99F G
var:: 
now:: 
=:: 
DateTime:: "
.::" #
UtcNow::# )
;::) *
var;; 
codeInfo;; 
=;; 
new;; "
CodeInfo;;# +
{<< 
Code== 
=== 
code== 
,==  
CreationTime>>  
=>>! "
now>># &
,>>& '
ExpirationTime?? "
=??# $
now??% (
.??( )

AddMinutes??) 3
(??3 4
$num??4 5
)??5 6
,??6 7
Type@@ 
=@@ 
type@@ 
}AA 
;AA 
_codeStorageBB 
[BB 
keyBB  
]BB  !
=BB" #
codeInfoBB$ ,
;BB, -
returnCC 
codeCC 
;CC 
}DD 
}EE 	
publicGG 
boolGG 
ValidateCodeGG  
(GG  !!
CodeValidationRequestGG! 6
requestGG7 >
)GG> ?
{HH 	
varII 
keyII 
=II 
GetKeyII 
(II 
requestII $
.II$ %

IdentifierII% /
,II/ 0
(II1 2
CodeTypeII2 :
)II: ;
requestII< C
.IIC D
CodeTypeIID L
)IIL M
;IIM N
lockKK 
(KK 
_lockObjectKK 
)KK 
{LL 
ifMM 
(MM 
!MM 
_codeStorageMM !
.MM! "
TryGetValueMM" -
(MM- .
keyMM. 1
,MM1 2
outMM3 6
CodeInfoMM7 ?
codeInfoMM@ H
)MMH I
)MMI J
{NN 
returnOO 
falseOO  
;OO  !
}PP 
ifRR 
(RR 
DateTimeRR 
.RR 
UtcNowRR #
>=RR$ &
codeInfoRR' /
.RR/ 0
ExpirationTimeRR0 >
)RR> ?
{SS 
returnTT 
falseTT  
;TT  !
}UU 
ifWW 
(WW 
codeInfoWW 
.WW 
TypeWW !
!=WW" $
(WW% &
CodeTypeWW& .
)WW. /
requestWW0 7
.WW7 8
CodeTypeWW8 @
)WW@ A
{XX 
returnYY 
falseYY  
;YY  !
}ZZ 
if\\ 
(\\ 
codeInfo\\ 
.\\ 
Code\\ !
!=\\" $
request\\% ,
.\\, -
Code\\- 1
)\\1 2
{]] 
return^^ 
false^^  
;^^  !
}__ 
ifaa 
(aa 
requestaa 
.aa 
Consumeaa #
)aa# $
{bb 
_codeStoragecc  
.cc  !
Removecc! '
(cc' (
keycc( +
)cc+ ,
;cc, -
}dd 
returnff 
trueff 
;ff 
}gg 
}hh 	
privatejj 
staticjj 
stringjj 
GetKeyjj $
(jj$ %
stringjj% +

identifierjj, 6
,jj6 7
CodeTypejj8 @
typejjA E
)jjE F
{kk 	
returnll 
$"ll 
{ll 
typell 
}ll 
$strll 
{ll 

identifierll '
}ll' (
"ll( )
;ll) *
}mm 	
}nn 
}oo ±
VC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Helpers\ResponseHelper.cs
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
ResponseHelper

 &
{ 
public 
static 
void 
SendResponse '
<' (
T( )
>) *
(* +
Action+ 1
<1 2
ServiceResponse2 A
<A B
TB C
>C D
>D E
callbackF N
,N O
ResponseInfoP \
<\ ]
T] ^
>^ _
info` d
)d e
{ 	
var 
response 
= 
new 
ServiceResponse .
<. /
T/ 0
>0 1
(1 2
)2 3
{ 
Success 
= 
info 
. 
Success &
,& '
Code 
= 
info 
. 
MessageCode '
,' (
Data 
= 
info 
. 
Data  
} 
; 
callback 
? 
. 
Invoke 
( 
response %
)% &
;& '
if 
( 
! 
string 
. 
IsNullOrWhiteSpace *
(* +
info+ /
./ 0

LogMessage0 :
): ;
); <
{ 
Logger 
. 
Log 
( 
info 
.  

LogMessage  *
)* +
;+ ,
} 
} 	
} 
} Ÿ
aC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Helpers\PendingRegistrationHelper.cs
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

	interface

 &
IPendingRegistrationHelper

 /
{ 
void $
StorePendingRegistration %
(% &
string& ,
email- 2
,2 3
PendingRegistration4 G
dataH L
)L M
;M N
PendingRegistration +
GetAndRemovePendingRegistration ;
(; <
string< B
emailC H
)H I
;I J
} 
public 

class %
PendingRegistrationHelper *
:+ ,&
IPendingRegistrationHelper- G
{ 
private 
static 
readonly 
Lazy  $
<$ %%
PendingRegistrationHelper% >
>> ?
	_instance@ I
=J K
new 
Lazy 
< %
PendingRegistrationHelper .
>. /
(/ 0
(0 1
)1 2
=>3 5
new6 9%
PendingRegistrationHelper: S
(S T
)T U
)U V
;V W
private 
readonly 
object 
_lockObject  +
=, -
new. 1
object2 8
(8 9
)9 :
;: ;
private 
readonly 

Dictionary #
<# $
string$ *
,* +
PendingRegistration, ?
>? @ 
pendingRegistrationsA U
=V W
new 

Dictionary 
< 
string !
,! "
PendingRegistration# 6
>6 7
(7 8
)8 9
;9 :
public 
static &
IPendingRegistrationHelper 0
Instance1 9
=>: <
	_instance= F
.F G
ValueG L
;L M
private %
PendingRegistrationHelper )
() *
)* +
{, -
}. /
public 
PendingRegistration "+
GetAndRemovePendingRegistration# B
(B C
stringC I
emailJ O
)O P
{ 	
lock 
( 
_lockObject 
) 
{ 
if   
(    
pendingRegistrations   (
.  ( )
TryGetValue  ) 4
(  4 5
email  5 :
,  : ;
out  < ?
PendingRegistration  @ S
data  T X
)  X Y
)  Y Z
{!!  
pendingRegistrations"" (
.""( )
Remove"") /
(""/ 0
email""0 5
)""5 6
;""6 7
return## 
data## 
;##  
}$$ 
return%% 
null%% 
;%% 
}&& 
}'' 	
public)) 
void)) $
StorePendingRegistration)) ,
()), -
string))- 3
email))4 9
,))9 :
PendingRegistration)); N
data))O S
)))S T
{** 	
lock++ 
(++ 
_lockObject++ 
)++ 
{,,  
pendingRegistrations-- $
[--$ %
email--% *
]--* +
=--, -
data--. 2
;--2 3
}.. 
}// 	
}00 
}11 ¶
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
} ﬂ
ZC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Helpers\NotificationSender.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 
Helpers %
{ 
public		 

	interface		 
INotificationSender		 (
{

 
Task -
!SendAccountVerificationEmailAsync .
(. /
string/ 5
email6 ;
,; <
string= C
codeD H
)H I
;I J
Task '
SendPasswordResetEmailAsync (
(( )
string) /
email0 5
,5 6
string7 =
code> B
)B C
;C D
} 
public 

class 
NotificationSender #
:$ %
INotificationSender& 9
{ 
private 
readonly 
IEmailSender %
_sender& -
;- .
private 
static 
readonly 
Lazy  $
<$ %
NotificationSender% 7
>7 8
	_instance9 B
=C D
new 
Lazy 
< 
NotificationSender '
>' (
(( )
() *
)* +
=>, .
new/ 2
NotificationSender3 E
(E F
)F G
)G H
;H I
public 
static 
INotificationSender )
Instance* 2
=>3 5
	_instance6 ?
.? @
Value@ E
;E F
private 
NotificationSender "
(" #
)# $
{ 	
_sender 
= 
new 
EmailSender %
(% &
)& '
;' (
} 	
public 
Task -
!SendAccountVerificationEmailAsync 5
(5 6
string6 <
email= B
,B C
stringD J
codeK O
)O P
{ 	
var 
subject 
= 
$str <
;< =
var 
body 
= 
$" 
$str R
"R S
+T U
$" 
$str X
"X Y
+Z [
$"   
$str   5
{  5 6
code  6 :
}  : ;
$str  ; @
"  @ A
+  B C
$"!! 
$str!! P
"!!P Q
;!!Q R
return"" 
_sender"" 
."" 
SendEmailAsync"" )
("") *
email""* /
,""/ 0
subject""1 8
,""8 9
body"": >
)""> ?
;""? @
}## 	
public%% 
Task%% '
SendPasswordResetEmailAsync%% /
(%%/ 0
string%%0 6
email%%7 <
,%%< =
string%%> D
code%%E I
)%%I J
{&& 	
var'' 
subject'' 
='' 
$str'' D
;''D E
var(( 
body(( 
=(( 
$"(( 
$str(( P
"((P Q
+((R S
$")) 
$str)) L
"))L M
+))N O
$"** 
$str** 5
{**5 6
code**6 :
}**: ;
$str**; @
"**@ A
+**B C
$"++ 
$str++ P
"++P Q
;++Q R
return,, 
_sender,, 
.,, 
SendEmailAsync,, )
(,,) *
email,,* /
,,,/ 0
subject,,1 8
,,,8 9
body,,: >
),,> ?
;,,? @
}-- 	
}.. 
}// ê
NC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Helpers\Logger.cs
	namespace		 	
UnoLisServer		
 
.		 
Common		 
.		 
Helpers		 %
{

 
public 

static 
class 
Logger 
{ 
private 
static 
readonly 
string  &
logFile' .
=/ 0
	Constants1 :
.: ;
	Constants; D
.D E
LogFileNameE P
;P Q
public 
static 
void 
Log 
( 
string %
message& -
)- .
{ 	
string 
entry 
= 
$" 
$str 
{ 
DateTime '
.' (
Now( +
:+ ,
$str, ?
}? @
$str@ B
{B C
messageC J
}J K
"K L
;L M
Console 
. 
	WriteLine 
( 
entry #
)# $
;$ %
try 
{ 
File 
. 
AppendAllText "
(" #
logFile# *
,* +
entry, 1
+2 3
Environment4 ?
.? @
NewLine@ G
)G H
;H I
} 
catch 
( 
IOException 
) 
{ 
Console 
. 
	WriteLine !
(! "
$str" N
)N O
;O P
} 
} 	
} 
} ≠
SC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Helpers\EmailSender.cs
	namespace		 	
UnoLisServer		
 
.		 
Common		 
.		 
Helpers		 %
{

 
public 

	interface 
IEmailSender !
{ 
Task 
SendEmailAsync 
( 
string "
recipientEmail# 1
,1 2
string3 9
subject: A
,A B
stringC I
bodyJ N
)N O
;O P
} 
public 

class 
EmailSender 
: 
IEmailSender +
{ 
public 
async 
Task 
SendEmailAsync (
(( )
string) /
recipientEmail0 >
,> ?
string@ F
subjectG N
,N O
stringP V
bodyW [
)[ \
{ 	
var 
smtpUser 
= 
Environment &
.& '"
GetEnvironmentVariable' =
(= >
$str> I
)I J
??K M 
ConfigurationManagerN b
.b c
AppSettingsc n
[n o
$stro y
]y z
;z {
var 
smtpPass 
= 
Environment &
.& '"
GetEnvironmentVariable' =
(= >
$str> I
)I J
??K M 
ConfigurationManagerN b
.b c
AppSettingsc n
[n o
$stro y
]y z
;z {
var 
smtpHost 
=  
ConfigurationManager /
./ 0
AppSettings0 ;
[; <
$str< F
]F G
;G H
var 
smtpPort 
= 
int 
. 
Parse $
($ % 
ConfigurationManager% 9
.9 :
AppSettings: E
[E F
$strF P
]P Q
)Q R
;R S
var 
message 
= 
new 
MailMessage )
{ 
From 
= 
new 
MailAddress &
(& '
smtpUser' /
,/ 0
$str1 :
): ;
,; <
Subject 
= 
subject !
,! "

IsBodyHtml 
= 
true !
,! "
Body 
= 
body 
} 
; 
message   
.   
To   
.   
Add   
(   
recipientEmail   )
)  ) *
;  * +
using"" 
("" 
var"" 

smtpClient"" !
=""" #
new""$ '

SmtpClient""( 2
(""2 3
smtpHost""3 ;
,""; <
smtpPort""= E
)""E F
)""F G
{## 

smtpClient$$ 
.$$ 
Credentials$$ &
=$$' (
new$$) ,
System$$- 3
.$$3 4
Net$$4 7
.$$7 8
NetworkCredential$$8 I
($$I J
smtpUser$$J R
,$$R S
smtpPass$$T \
)$$\ ]
;$$] ^

smtpClient%% 
.%% 
	EnableSsl%% $
=%%% &
true%%' +
;%%+ ,
await&& 

smtpClient&&  
.&&  !
SendMailAsync&&! .
(&&. /
message&&/ 6
)&&6 7
;&&7 8
}'' 
}(( 	
})) 
}** –
^C:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Exceptions\ValidationException.cs
	namespace 	
UnoLisServer
 
. 
Common 
. 

Exceptions (
{		 
public

 

class

 
ValidationException

 $
:

% &
	Exception

' 0
{ 
public 
MessageCode 
	ErrorCode $
{% &
get' *
;* +
private, 3
set4 7
;7 8
}9 :
public 
ValidationException "
(" #
MessageCode# .
	errorCode/ 8
,8 9
string: @
messageA H
=I J
nullK O
)O P
:Q R
baseS W
(W X
messageX _
)_ `
{ 	
	ErrorCode 
= 
	errorCode !
;! "
} 	
} 
} ≈;
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
, 
PlayerReady 
= 
$num 
, 
PlayerNotReady 
= 
$num 
, 
RewardGranted 
= 
$num 
, 
PurchaseCompleted 
= 
$num  
,  ! 
VerificationCodeSent   
=   
$num   #
,  # $"
VerificationCodeResent!! 
=!!  
$num!!! %
,!!% &

BadRequest$$ 
=$$ 
$num$$ 
,$$ 
InvalidData%% 
=%% 
$num%% 
,%% 
InvalidCredentials&& 
=&& 
$num&& !
,&&! " 
NicknameAlreadyTaken'' 
='' 
$num'' #
,''# $"
EmailAlreadyRegistered(( 
=((  
$num((! %
,((% &
InvalidEmailFormat)) 
=)) 
$num)) !
,))! "
WeakPassword** 
=** 
$num** 
,** 
PlayerNotFound++ 
=++ 
$num++ 
,++ 
SamePassword,, 
=,, 
$num,, 
,,, 
EmptyMessage-- 
=-- 
$num-- 
,-- 
MessageTooLong.. 
=.. 
$num.. 
,..  
InappropriateContent// 
=// 
$num// #
,//# $
AlreadyFriends00 
=00 
$num00 
,00  
PendingFriendRequest11 
=11 
$num11 #
,11# $
InvalidSocialUrl22 
=22 
$num22 
,22  
BlockedUser33 
=33 
$num33 
,33 
LobbyNotFound44 
=44 
$num44 
,44 
	LobbyFull55 
=55 
$num55 
,55 !
OperationNotSupported66 
=66 
$num66  $
,66$ %
ValidationFailed77 
=77 
$num77 
,77  
EmptyFields88 
=88 
$num88 
,88 
RateLimitExceeded99 
=99 
$num99  
,99  ! 
RegistrationDataLost:: 
=:: 
$num:: #
,::# $#
VerificationCodeInvalid;; 
=;;  !
$num;;" &
,;;& '
SessionExpired>> 
=>> 
$num>> 
,>> 
UnauthorizedAccess?? 
=?? 
$num?? !
,??! "
InvalidToken@@ 
=@@ 
$num@@ 
,@@ 
MissingTokenAA 
=AA 
$numAA 
,AA 
DuplicateSessionBB 
=BB 
$numBB 
,BB  
UserNotConnectedCC 
=CC 
$numCC 
,CC  
LoginInternalErrorDD 
=DD 
$numDD !
,DD! "
LogoutInternalErrorEE 
=EE 
$numEE "
,EE" #
DatabaseErrorHH 
=HH 
$numHH 
,HH 
TransactionFailedII 
=II 
$numII  
,II  !
SqlErrorJJ 
=JJ 
$numJJ 
,JJ 
ConcurrencyConflictKK 
=KK 
$numKK "
,KK" #
SerializationErrorLL 
=LL 
$numLL !
,LL! "
UnhandledExceptionMM 
=MM 
$numMM !
,MM! "
CallbackErrorNN 
=NN 
$numNN 
,NN 
ProfileUpdateFailedOO 
=OO 
$numOO "
,OO" #
ProfileFetchFailedPP 
=PP 
$numPP !
,PP! "
ChatInternalErrorQQ 
=QQ 
$numQQ  
,QQ  ! 
FriendsInternalErrorRR 
=RR 
$numRR #
,RR# $
LobbyInternalErrorSS 
=SS 
$numSS !
,SS! "
GeneralServerErrorTT 
=TT 
$numTT !
,TT! "
ConnectionLostWW 
=WW 
$numWW 
,WW 
TimeoutXX 
=XX 
$numXX 
,XX 
ConnectionFailedYY 
=YY 
$numYY 
,YY  
ConnectionRejectedZZ 
=ZZ 
$numZZ !
,ZZ! "
UnstableConnection[[ 
=[[ 
$num[[ !
,[[! "
ClientDisconnected\\ 
=\\ 
$num\\ !
,\\! "!
FriendActionCompleted__ 
=__ 
$num__  $
,__$ %
PlayerBlocked`` 
=`` 
$num`` 
,`` 
PlayerUnblockedaa 
=aa 
$numaa 
,aa  
PlayerHasActiveLobbybb 
=bb 
$numbb #
,bb# $
PlayerNotInLobbycc 
=cc 
$numcc 
,cc  
PlayerAlreadyReadydd 
=dd 
$numdd !
,dd! "
PlayerWereNotReadyee 
=ee 
$numee !
,ee! "
MatchAlreadyStartedff 
=ff 
$numff "
,ff" #
MatchCancelledgg 
=gg 
$numgg 
,gg 
MatchNotFoundhh 
=hh 
$numhh 
,hh 
MatchAlreadyEndedii 
=ii 
$numii  
,ii  !
PlayerKickedjj 
=jj 
$numjj 
,jj 
PlayerBannedkk 
=kk 
$numkk 
,kk 
LobbyClosedll 
=ll 
$numll 
,ll 
NoPermissionsmm 
=mm 
$nummm 
,mm "
LobbyInconsistentStatenn 
=nn  
$numnn! %
,nn% &
PlayerDisconnectedoo 
=oo 
$numoo !
,oo! "
PlayerReconnectedpp 
=pp 
$numpp  
,pp  ! 
MatchResultsRecordedqq 
=qq 
$numqq #
,qq# $!
RewardProcessingErrorrr 
=rr 
$numrr  $
,rr$ %#
PurchaseProcessingErrorss 
=ss  !
$numss" &
}tt 
}uu ≠
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
} Å
NC:\Users\meler\Source\Repos\UnoLisServer\UnoLisServer.Common\Enums\CodeType.cs
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
enum		 
CodeType		 
{

 
EmailVerification 
, 
PasswordReset 
} 
} û
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
}!! É
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
} §
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
$str	k ¶
"
¶ ß
;
ß ®
} 	
} 
} 