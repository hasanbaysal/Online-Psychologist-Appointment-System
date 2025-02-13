
# Online Psychologist Appointment System #

<br/>
<br/>
<br/>

### 13.02.2025 Tarihi itibariyle projeyi güncelleme kararı aldım  zaman içerisinde aşağıdaki özellikleri eklemeyi düşünüyorum
- Açık kaynak ürünler kullanarak kendi dahili video chat sistemizi yazacağız. mediaserver =>  ( jitsi, bigbluebutton vb)
- Mimarisel değşimler  Service-Based Architecture  + Event-Driven Scaling gibi bir mimari düşünüyorum, direkt micro yazmak gereksiz burada tek scale edilmesi gereken sistem video görüşmeleri için media server olacak. Event based bir şekilde yüke göre scale edeceğim, diğer operasyonları kullanıcı yönetimi, ödeme, randevuları vs. monolith veya modüler bir serviste yönetebiliriz boşuna micro yazmaya gerek yok
- UI tarafında mvc'den devam etmek yerine react. orada da signalr tarzı teknolojiler ile kullanıcı ve psikolog etkileşimlerini gerçek zamanlı hale getirebiliriz
- mail operasyonlarını için vs Rabbit gibi message queue araçları kullanmak
- veritabanı olarak posgresql'ye geçiş
- verimli loglama sistemleri kullanmak
- uygulama yük trafik ve sağlık durumu için monitörleme
  
<br/>
Yani mimarisel büyük bir değişim düşünüyorum uygulama özelinde vakit buldukça değişime gideceğim. İşlevsel olarak uygulamaya ek birkaç özellik eklemek içinda genel işleyişi bozmayacağım

<br/><br/><br/>

 



### Codes for a project of mine that is not currently live ###

Some features of the project and technologies used:
* Asp.net core mvc
* Ef-core
* N-tier architecture
* Automapper
* Fluent Validation
* Asp.net Core Identity Framework
* 3 party payment systems
* 3d Secure
* Sign in with Google
* Registration with Google
* SMS services
* Video Call - 3.part api daly.co integration
* Admin Dashboard
* Psychologist Dashboard
* in memory cache
* custom mail services

	[Youtube Video](https://www.youtube.com/watch?v=iihOnjF1qQw)

![ss](https://github.com/hasanbaysal/Online-Psikolog-Sistemi/blob/master/ss.png)

