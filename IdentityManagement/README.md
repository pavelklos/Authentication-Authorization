[**HOME**]
/

[**REGISTER**] => .AspNetCore.Cookies
/**register**?username=**bob**&password=**password**
[**LOGIN**] => .AspNetCore.Cookies 
/**login**?username=**bob**&password=**password**
[**LOGIN**] **bad credentials**
/**login**?username=**bob**&password=**password1**

[**PROMOTE**]
/**promote**?username=**bob**
[**PROTECTED**]
/**protected**

[**START-PASSWORD-RESET**]
/**start-password-reset**?username=**bob**
[**END-PASSWORD-RESET**]
/**end-password-reset**?username=**bob**&password=**password**&hash=**PROTECTED-USERNAME**
