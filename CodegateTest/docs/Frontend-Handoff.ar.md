
تاريخ المراجعة: **7 أكتوبر 2026**. المرجع هو الكود الحالي في المشروع، وليس التعديلات المقترحة التي لم تُطبّق.

هذا الدليل يشرح الاستخدام الفعلي للـ API. أمثلة البيانات توضيحية، ولا تحتوي بيانات دخول حقيقية. مراجعة العقود ونجاح البناء لا تعني أن جميع المسارات اختُبرت على قاعدة البيانات والبريد الفعليين.

## محتويات التسليم

- هذا الملف: تشغيل الفرونت، الصلاحيات، النماذج، المسارات والاستجابات، والقيود الحالية.
- [Postman Collection](./CodegateTest.postman_collection.json): جميع المسارات، مع متغيرات وإرسال JSON وFormData.
- [قائمة المسارات](./api-endpoints.json): مرجع قابل للقراءة برمجيًا، مستخرج من تعريفات الكنترولرز.

## 1. بداية التشغيل

عنوان الباك المحلي عند تشغيل HTTPS profile:

```text
https://localhost:7204
```

يوجد HTTP profile على `http://localhost:5169`، لكن المشروع يفعّل HTTPS redirection؛ يُفضّل استخدام عنوان HTTPS في الفرونت وPostman وقبول شهادة التطوير المحلية. عنوان النشر يُؤخذ من مسؤول الباك، ولا يُفترض أنه نفس عنوان التطوير.

في بيئة Development:

```text
GET /scalar/v1
GET /openapi/v1.json
```

وثائق Scalar وOpenAPI غير مفعّلة في Production حاليًا. هذه روابط افتراضية وفق تسجيل الأدوات في المشروع، ويجب تأكيد الوصول عند تشغيل الباك.

عناوين الفرونت المسموح بها في CORS حاليًا هي بالضبط:

```text
http://127.0.0.1:5500
http://localhost:5500
http://localhost:4200
http://localhost:5173
```

لو الفرونت يعمل على منفذ أو بروتوكول أو دومين مختلف، يحتاج مسؤول الباك إضافته. `localhost` و`127.0.0.1` ليسا نفس Origin.

قبل الربط، مسؤول الباك يتأكد من إعداد SQL Server والبريد وJWT، وتطبيق migrations، خصوصًا إضافة جدول OTP وعمود FailedAttempts. لا يُسلَّم للفرونت مفتاح JWT أو بيانات SMTP أو حساب الأدمن الحقيقي. اطلب حسابي اختبار: Student ببريد مؤكّد وAdmin.

## 2. قواعد الطلبات والصلاحيات

- أسماء حقول JSON في هذا الدليل هي الأسماء الخارجة فعليًا بصيغة camelCase.
- المعرفات: المستخدم نص `string`، والكورس والمدرس والتقييم والرسالة أعداد صحيحة.
- طلبات JSON تستخدم `Content-Type: application/json`.
- رفع الملفات يستخدم `multipart/form-data`؛ عند استخدام FormData لا تضبط Content-Type يدويًا، لأن المتصفح يضيف boundary.
- `Public`: لا يحتاج توكن، حتى لو اسم المسار يحتوي `/Admin/`.
- `Authenticated`: يحتاج توكن حساب مسجّل، سواء Student أو Admin.
- `Student`: يحتاج دور Student تحديدًا. دور Admin وحده لا يعطيه هذه الصلاحية.
- `Admin`: يحتاج دور Admin.

في الطلبات المحمية:

```http
Authorization: Bearer <JWT>
```

الفرونت يوجّه المستخدم حسب الدور، لكن الباك هو المسؤول عن تطبيق الصلاحيات.

## 3. التعامل مع الاستجابات والأخطاء

معظم عمليات الكتابة والحسابات تستخدم غلاف APIResponce:

```json
{
  "uuid": "example-request-id",
  "statusCode": 200,
  "message": ["Operation completed successfully."],
  "data": null,
  "dateTime": "2026-10-07T12:00:00+03:00"
}
```

`message` مصفوفة نصوص وقد تكون null. `data` قد تكون نصًا، كائنًا، مصفوفة أو null حسب المسار. `uuid` ليس معرف المستخدم أو الكيان.

**لا تفترض أن كل المسارات ترجع هذا الغلاف.** قوائم الكورسات والمستخدمين والرسائل، بيانات البروفايل، المدرسون، والـ Dashboard لها أشكال مستقلة موضحة أدناه. بعض نجاحات إدارة المستخدمين ترجع `message` كنص واحد.

أخطاء الـ DTO Validation ترجع HTTP 400 بصيغة ASP.NET ValidationProblemDetails، مثل:

```json
{
  "type": "about:blank",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Rating": ["The field Rating must be between 1 and 5."]
  },
  "traceId": "example-trace-id"
}
```

قيمة `type` و`traceId` ورسائل التحقق تتغير؛ المثال يوضح الشكل فقط. مفاتيح `errors` قد تكون أسماء C# مثل `Rating` وليست camelCase. أخطاء تحديث البروفايل من Identity قد ترجع مصفوفة مباشرة من `{code, description}`.

تعامل مع كود HTTP الفعلي أولًا:

- 200: نجاح القراءة أو التعديل أو الحذف.
- 201: نجاح الإنشاء.
- 400: بيانات غير صحيحة، توكن/OTP غير صالح، أو عملية مرفوضة.
- 401: توكن غائب/منتهي/غير صالح، أو رفض Login. اقرأ رسالة Login؛ قد يكون السبب عدم تأكيد البريد أو قفل الحساب.
- 403: الدور غير مناسب، أو محاولة تعديل/حذف تقييم طالب آخر.
- 404: العنصر غير موجود أو محذوف في المسارات التي تفحص الحذف المنطقي.
- 500: مشكلة في السيرفر أو حفظ العملية.
- 503: إرسال بريد إعادة التأكيد غير متاح.

401 و403 الناتجان من middleware وبعض فروع الكنترولرز قد يكونان بدون JSON. والأخطاء غير المعالجة قد ترجع نصًا بدل JSON، خصوصًا في Development؛ كود الفرونت يجب ألا يفترض وجود body صالح دائمًا.

تواريخ الكيانات مثل `createdAt` تستخدم UTC في الإنشاء. `dateTime` داخل الغلاف يستخدم وقت السيرفر المحلي. اعرض التواريخ باستخدام أدوات التاريخ، ولا تعتمد على أن كل القيم لها نفس المنطقة الزمنية.

## 4. الحسابات وتسجيل الدخول

جميع المسارات في هذا القسم Public، تحت `/api/Identity/Accounts`.

### POST /api/Identity/Accounts/Register

Body: JSON.

```json
{
  "fname": "Ahmed",
  "lname": "Ali",
  "email": "student@example.com",
  "userName": "ahmed.ali",
  "password": "Example@123",
  "confirmPassword": "Example@123"
}
```

الحقول كلها مطلوبة. الأسماء 2–50 حرفًا، البريد صالح وبحد أقصى 255، اليوزر بحد أقصى 256 وبحروف إنجليزية/أرقام أو `@ . _ + -`، بدون مسافات. الباسورد 8–100 حرف وتأكيده مطابق. Identity يطبّق أيضًا سياسة قوة الباسورد: حرف كبير وصغير ورقم ورمز. اليوزر والبريد يجب ألا يتعارضا مع حساب موجود؛ البريد فريد.

نجاح 201 داخل APIResponce:

```json
{
  "userId": "example-user-id",
  "emailConfirmationSent": true
}
```

الكائن أعلاه هو **data**. الحساب يأخذ دور Student تلقائيًا، ويحتاج تأكيد البريد قبل Login.

إذا `emailConfirmationSent` كانت false، الحساب تم إنشاؤه؛ لا تعِد التسجيل. احتفظ بـ `data.userId` ووفّر زر إعادة إرسال التأكيد. فشل التسجيل قد يرجع 400 أو 500؛ رسالة 500 قد تنبه إلى وجود حساب غير مكتمل.

### POST /api/Identity/Accounts/Login

Body: JSON. الدخول باسم المستخدم والباسورد فقط؛ لا يوجد حقل email في طلب الدخول.

```json
{
  "userName": "ahmed.ali",
  "password": "Example@123",
  "rememberMe": false
}
```

`userName` و`password` مطلوبان، `rememberMe` اختياري وافتراضيًا false.

نجاح 200:

```json
{
  "uuid": "example-request-id",
  "statusCode": 200,
  "message": ["Welcome, ahmed.ali"],
  "data": "<JWT>",
  "dateTime": "2026-10-07T12:00:00+03:00"
}
```

التوكن هو `response.data` مباشرةً، وليس `response.data.token`. مدة JWT خمسة عشر دقيقة؛ `rememberMe` لا يطيلها. لا يوجد Refresh Token أو Logout endpoint. تسجيل الخروج في الواجهة يمسح التوكن محليًا. بعد انتهائه يلزم Login جديد.

JWT يحتوي معرف المستخدم والاسم والبريد والأدوار و`exp`. أسماء claims الحالية تشمل:

```text
http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name
http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress
http://schemas.microsoft.com/ws/2008/06/identity/claims/role
```

استخدم مكتبة JWT decode للقراءة في الواجهة؛ الـ decode لا يتحقق من التوقيع ولا يعطي صلاحية بذاته. claim الدور قد يكون نصًا أو مصفوفة لو للحساب عدة أدوار. لا يوجد `/me` يعيد الدور؛ `/api/Profile` يعيد البروفايل فقط.

### GET /api/Identity/Accounts/Confirm

Query مطلوب: `userId` و`token`، مأخوذان من رابط التأكيد المُرسل بالبريد. لا يوجد body.

```text
/api/Identity/Accounts/Confirm?userId=<id>&token=<encoded-token>
```

استخدم URLSearchParams للحفاظ على الرموز مثل `+` داخل التوكن. النجاح 200 داخل APIResponce، التوكن غير الصحيح/المنتهي 400، المستخدم غير الموجود 404.

رابط البريد الحالي يشير إلى الباك ويعرض JSON؛ لا يوجد redirect تلقائي إلى صفحة الفرونت. لو أردت صفحة تأكيد داخل الواجهة، يُنسّق رابط البريد مع مسؤول الباك.

### GET /api/Identity/Accounts/ResendEmailConfirmation

Query مطلوب: `userId`. بدون body. نجاح 200، بريد مؤكّد بالفعل 400، مستخدم غير موجود 404، فشل إرسال 503. المسار يستخدم GET في النسخة الحالية رغم أنه يرسل بريدًا؛ نادِه من زر المستخدم، وليس من تحميل الصفحة تلقائيًا أو prefetch.

## 5. استعادة كلمة المرور

التسلسل المطلوب: Forget-Password → Validate-OTP → Reset-Password → Login.

### POST /api/Identity/Accounts/Forget-Password

Public، JSON:

```json
{ "email": "student@example.com" }
```

البريد مطلوب وصالح وبحد أقصى 255. نجاح 200، داخل data:

```json
{ "applicationUserId": "example-user-id" }
```

احفظ المعرف للشاشتين التاليتين. OTP يُرسل للبريد ولا يرجع في JSON. الكود أربعة أرقام، صالح 10 دقائق. إصدار كود جديد يلغي السابق. الحد الحالي **50 إصدارًا خلال آخر 24 ساعة**، وليس 10؛ تجاوزه يرجع 400. المستخدم غير الموجود يرجع 404.

### POST /api/Identity/Accounts/Validate-OTP

Public، JSON:

```json
{
  "applicationUserId": "example-user-id",
  "otp": "1234"
}
```

الحقول مطلوبة. `otp` نص من أربعة أرقام، وليس number. يُقبل آخر كود فقط، غير مستخدم وغير منتهٍ. بعد خمس محاولات خاطئة يُلغى ويلزم طلب كود جديد.

نجاح 200، داخل data:

```json
{
  "applicationUserId": "example-user-id",
  "resetToken": "<password-reset-token>"
}
```

بعد النجاح يُستهلك OTP. احفظ resetToken مؤقتًا للشاشة التالية؛ لا تخلطه مع JWT أو توكن تأكيد البريد. فشل التحقق 400، المستخدم غير الموجود 404، فشل حفظ النتيجة 500.

### POST /api/Identity/Accounts/Reset-Password

Public، JSON:

```json
{
  "applicationUserId": "example-user-id",
  "resetToken": "<password-reset-token>",
  "password": "NewExample@123",
  "confirmPassword": "NewExample@123"
}
```

كل الحقول مطلوبة، الباسورد 8–100 حرف مع سياسة Identity، والتأكيد مطابق. ResetToken صالح **10 دقائق من توليده عند نجاح Validate-OTP**؛ مدة مستقلة عن عداد صلاحية OTP.

نجاح 200 داخل APIResponce بدون توكن Login جديد. وجّه المستخدم إلى Login. فشل Identity يرجع 400 مع أوصاف الأخطاء، والمستخدم غير الموجود 404. لو سياسة الباسورد رفضت الطلب والتوكن ما زال صالحًا، يمكن تصحيح الباسورد وإعادة نفس طلب Reset-Password.

## 6. البروفايل

### GET /api/Profile

Authenticated، بدون body. استجابة 200 **مباشرة**:

```json
{
  "fullName": "Ahmed Ali",
  "email": "student@example.com",
  "profileImage": "/img/profiles/example.png"
}
```

`profileImage` قد تكون رابط DiceBear خارجيًا، مسارًا محليًا أو null. المستخدم غير الموجود 404. الاستجابة لا تحتوي `userName` أو الدور أو `fname/lname` منفصلين؛ لا تفترض أن تقسيم fullName بالمسافة يعيد الاسمين بدقة.

### PUT /api/Profile

Authenticated، FormData. أسماء الحقول:

- `Fname`: اختياري، 2–50 حرفًا، ليس مسافات فقط.
- `Lname`: اختياري، نفس القيود.
- `ProfileImage`: ملف اختياري.

أرسل فقط ما تريد تغييره. إرسال الصورة وحدها يحافظ على الأسماء. نجاح 200 داخل APIResponce، صورة غير صالحة 400، مستخدم غير موجود 404. لا توجد عملية مستقلة لإزالة الصورة دون رفع بديل.

## 7. الكورسات

### GET /api/Admin/Courses

Public رغم `/Admin/`. Query: `page`، افتراضيًا 1؛ القيم الأقل من 1 تتحول إلى 1. حجم الصفحة ثابت **5**، لا يوجد pageSize مخصص أو search للكورسات.

الترتيب: createdAt تنازلي ثم id تصاعدي. استجابة 200 مباشرة:

```json
{
  "items": [
    {
      "id": 1,
      "name": "Frontend course",
      "slug": "frontend-course",
      "price": 199.99,
      "description": "Course description",
      "isActive": true,
      "coverImageUrl": "/img/courses_img/example.jpg",
      "instructors": ["Ahmed Ali"]
    }
  ],
  "totalPages": 1,
  "totalCourses": 1,
  "pageSize": 5
}
```

الكورسات المحذوفة والمدرسون المحذوفون مخفيون. القائمة لا تفلتر `isActive=false` حاليًا؛ ده حقل حالة يرجع للواجهة، وليس شرط إخفاء مطبقًا في الباك. الصفحة بعد آخر صفحة ترجع items فارغة، وقائمة بلا نتائج قد يكون totalPages فيها 0.

### GET /api/Admin/Courses/{id}

Public، بدون body. 200 بنفس CoursesResponce، لكن **items كائن واحد وليس مصفوفة**:

```json
{
  "items": {
    "id": 1,
    "name": "Frontend course",
    "slug": "frontend-course",
    "price": 199.99,
    "description": "Course description",
    "isActive": true,
    "coverImageUrl": "/img/courses_img/example.jpg",
    "instructors": [
      { "name": "Ahmed Ali", "avatarUrl": "/img/instructors_img/example.png" }
    ]
  },
  "totalPages": 0,
  "totalCourses": 0,
  "pageSize": 0
}
```

أصفار Pagination في التفاصيل قيم افتراضية وليست بيانات صفحات. الكورس غير الموجود/المحذوف 404. المدرسون هنا كائنات name/avatarUrl، بينما في القائمة أسماء نصية. الاستجابتان لا ترجعان InstructorIds؛ شاشة تعديل ربط المدرسين لا يمكنها استخراج الربط الدقيق من أسماء قد تتكرر، ويحتاج ذلك تنسيقًا مع مسؤول الباك.

لا يوجد مسار GET بالـ slug. لو رابط الواجهة يستخدم slug، احفظ معه id أو نسّق إضافة مسار مناسب.

### POST /api/Admin/Courses

Admin، FormData:

- `Name`: مطلوب، 3–100 حرف.
- `Slug`: مطلوب، حتى 150؛ حروف إنجليزية صغيرة وأرقام وشرطة `-` فقط.
- `Price`: مطلوب، حد أدنى 0.01.
- `Description`: اختياري.
- `CoverImage`: ملف مطلوب.
- `InstructorIds`: معرف واحد على الأقل؛ كرر المفتاح لكل مدرس.

كل مدرس يجب أن يكون موجودًا وغير محذوف، وكل ID موجب وغير مكرر. الفشل 400 قبل رفع الصورة. نجاح 201 داخل APIResponce. **الاستجابة الحالية لا ترجع معرف الكورس الجديد**؛ أعد تحميل القائمة ولا تفترض أن data تحتوي id، ولا تفترض أن slug له قيد فريد في قاعدة البيانات.

```javascript
const form = new FormData();
form.append('Name', 'Frontend course');
form.append('Slug', 'frontend-course');
form.append('Price', '199.99');
form.append('CoverImage', file);
[1, 2].forEach(id => form.append('InstructorIds', String(id)));
```

### PUT /api/Admin/Courses/{id}

Admin، FormData. الحقول الاختيارية: `Name`, `Slug`, `Price`, `Description`, `IsActive`, **`CoverImg`** و`InstructorIds`.

اسم ملف التعديل هو CoverImg، مختلف عن CoverImage في الإنشاء. قيود الاسم/slug/السعر كما في الإنشاء. إن أرسلت InstructorIds فهي **القائمة النهائية المطلوبة**، وليست أسماء مدرسين لإضافتهم فقط. غير المرسل يحافظ على الربط؛ القائمة الفارغة أو المكررة أو غير الصحيحة مرفوضة. لا تستخدم JSON لهذا المسار.

نجاح 200 داخل APIResponce، غير موجود/محذوف 404، قائمة أو صورة غير صحيحة 400. رفع صورة جديدة يحافظ على القديمة حتى نجاح الحفظ. إرسال نص فارغ في FormData قد يتحول إلى null ويحافظ على القيمة الحالية؛ لا تستخدمه كطريقة مضمونة لمسح Description.

### DELETE /api/Admin/Courses/{id}

Admin، بدون body. حذف منطقي؛ نجاح 200 داخل APIResponce. غير موجود أو محذوف مسبقًا 404. لا يوجد استرجاع Restore endpoint، والحذف لا يعني حذف ملفات الصور أو تقييمات الكورس فعليًا.

## 8. المدرسون

شكل استجابة المدرس المباشرة:

```json
{
  "id": 1,
  "firstName": "Ahmed",
  "lastName": "Ali",
  "title": "Course instructor",
  "avatarUrl": "/img/instructors_img/example.png",
  "createdAt": "2026-10-07T12:00:00Z",
  "isDeleted": false,
  "courseInstructors": []
}
```

لا تعتمد على courseInstructors لاستنتاج كورسات المدرس؛ هذه navigation property وليست علاقة محمّلة بعقد ثابت في مسارات القراءة الحالية.

### GET /api/Admin/Instructors

Public، بدون body أو Pagination. 200 مصفوفة مباشرة من المدرسين غير المحذوفين، وليس APIResponce.

### GET /api/Admin/Instructors/{id}

Public، 200 كائن مدرس مباشر؛ غير موجود/محذوف 404.

### POST /api/Admin/Instructors

Admin، FormData: `FirstName` و`LastName` مطلوبان 2–50 حرفًا، `Title` مطلوب 3–100، وملف **`logo`** اختياري. نجاح 201 داخل APIResponce بدون id للمدرس؛ أعد تحميل قائمة المدرسين لاختياره بالمعرف. صورة غير صالحة 400، فشل الحفظ 500.

### PUT /api/Admin/Instructors/{id}

Admin، FormData، نفس الأسماء `FirstName`, `LastName`, `Title`, `logo`، وكلها اختيارية؛ قيود النصوص تطبق عند إرسالها. نجاح 200، غير موجود/محذوف 404. رفع بديل يحذف القديمة بعد نجاح الحفظ فقط.

### DELETE /api/Admin/Instructors/{id}

Admin، بدون body؛ حذف منطقي. نجاح 200، غير موجود/محذوف مسبقًا 404. لا يوجد Restore. المدرس المحذوف لا يظهر في عرض الكورسات ولا يقبل ربطًا جديدًا، لكن صفوف الربط التاريخية ليست بالضرورة محذوفة من قاعدة البيانات.

## 9. تقييمات الطالب

كل المسارات تحت `/api/Student/Reviews` تحتاج دور **Student**، حتى قراءة تقييمات الكورس. يوجد تقييم واحد لكل طالب لكل كورس، وفق فحص الكنترولر والقيد الموجود في قاعدة البيانات.

### POST /api/Student/Reviews

JSON:

```json
{ "courseId": 1, "feedback": "A useful course.", "rating": 5 }
```

courseId موجب، feedback مطلوب 3–2000 حرف، rating بين 1 و5. لا ترسل studentId؛ الباك يأخذه من JWT. الكورس المحذوف/غير الموجود 404، تقييم موجود مسبقًا 400. نجاح 201 داخل APIResponce، بحالة **Pending**.

**الإنشاء لا يعيد reviewId حاليًا**، ولا يوجد GET لتقييمات الطالب الشخصية أو الـ Pending الخاصة به. لذلك شاشة تعديل تقييمه فور الإنشاء لا تملك معرفًا موثوقًا من هذا الطلب؛ يلزم الاتفاق مع الباك على إرجاع المعرف أو إضافة مسار مناسب. لا تعتبر قائمة Approved قائمة تقييمات الطالب الشخصية.

### PUT /api/Student/Reviews/{id}

JSON مطلوب:

```json
{ "feedback": "Updated review feedback.", "rating": 4 }
```

نفس القيود، ولصاحب التقييم فقط. نجاح 200 داخل APIResponce. يرجع التقييم إلى Pending ويتحدث updatedAt. غير موجود 404، ليس صاحبه 403. المسار لا يسمح بتغيير courseId.

### DELETE /api/Student/Reviews/{id}

بدون body؛ لصاحبه فقط. نجاح 200، غير موجود 404، ملكية خاطئة 403. هذا **حذف فعلي للتقييم**؛ بعد نجاحه يمكن إنشاء تقييم جديد لنفس الكورس.

### GET /api/Student/Reviews/Course/{courseId}

Student، بدون body. غير موجود/محذوف 404. نجاح 200، data مصفوفة من **Approved فقط**:

```json
[
  {
    "id": 1,
    "rating": 5,
    "feedback": "A useful course.",
    "studentName": "Ahmed Ali",
    "studentImage": "https://api.dicebear.com/example.svg",
    "createdAt": "2026-10-07T12:00:00Z"
  }
]
```

لا توجد Pagination ولا متوسط تقييم محسوب من الباك. Pending وRejected لا يظهران هنا. السماح للزائر بقراءة التقييمات **لم يُطبّق**.

## 10. إدارة التقييمات

كل المسارات التالية Admin. قيم ReviewStatus: `0 = Pending`، `1 = Approved`، `2 = Rejected`. JSON يرجعها أرقامًا.

ReviewEntity داخل data يحتوي: `id`, `feedback`, `rating`, `reviewStatus`, `createdAt`, `updatedAt`, `studentId`, `courseId`, بالإضافة إلى navigation properties `student` و`course` التي قد تكون null. لا تعتمد عليهما للحصول على الاسم؛ يمكن استخدام مسارات المستخدم/الكورس المناسبة.

### GET /api/Admin/Reviews

Query اختياري: `status`. بدون الفلتر يرجع الكل؛ مثال `?status=0` للمراجعة. نجاح 200 داخل APIResponce، data مصفوفة ReviewEntity، بدون Pagination. أرسل 0/1/2؛ لا تعتمد على قبول أرقام Enum أخرى.

### GET /api/Admin/Reviews/{id}

نجاح 200 داخل APIResponce، data كائن ReviewEntity؛ غير موجود 404.

### PUT /api/Admin/Reviews/{id}/Approve

بدون body؛ يضبط الحالة Approved. نجاح 200 داخل APIResponce، غير موجود 404. لا يوجد body خاص للموافقة.

### PUT /api/Admin/Reviews/{id}/Reject

بدون body؛ يضبط الحالة Rejected. نجاح 200، غير موجود 404. لا يوجد حقل لسبب الرفض في النسخة الحالية.

### DELETE /api/Admin/Reviews/{id}

بدون body؛ حذف فعلي. نجاح 200، غير موجود 404.

## 11. التواصل ورسائل الأدمن

### POST /api/Contacts

Public، JSON:

```json
{
  "senderName": "Ahmed Ali",
  "email": "student@example.com",
  "phone": "+201000000000",
  "subject": "Course question",
  "message": "I would like more information about this course."
}
```

senderName مطلوب 3–100، email مطلوب صالح حتى 255، message مطلوب 10–5000، phone اختياري صالح حتى 20، subject اختياري حتى 150. نجاح 201 داخل APIResponce بدون contactId. إنشاء الرسالة يحفظها؛ لا يعني أن ردًا أو بريدًا أُرسل للمستخدم.

ContactEntity: `id`, `senderName`, `email`, `phone`, `subject`, `message`, `createdAt`, `isRead`.

### GET /api/Admin/Contacts

Admin، Query `page=1`، حجم 5؛ ترتيب createdAt تنازلي. نجاح 200 مباشر:

```json
{
  "page": 1,
  "totalMessagesCount": 1,
  "totalPages": 1,
  "items": [
    {
      "id": 1,
      "senderName": "Ahmed Ali",
      "email": "student@example.com",
      "phone": null,
      "subject": "Course question",
      "message": "I would like more information about this course.",
      "createdAt": "2026-10-07T12:00:00Z",
      "isRead": false
    }
  ]
}
```

لا يوجد فلتر بحث أو فلتر isRead حاليًا. الصفحة خارج النتائج ترجع items فارغة.

### GET /api/Admin/Contacts/{id}

Admin، 200 كائن ContactEntity مباشر؛ غير موجود 404. القراءة وحدها لا تعلّم الرسالة كمقروءة.

### PUT /api/Admin/Contacts/{id}

Admin، JSON، حقول اختيارية `senderName`, `email`, `phone`, `subject`, `message` بقيود الإنشاء عند إرسالها. غير المرسل يحافظ على القيمة. لا يوجد تعديل isRead هنا. نجاح 200 داخل APIResponce، غير موجود 404.

### PATCH /api/Admin/Contacts/{id}/read

Admin، بدون body؛ يجعل isRead=true. نجاح 200، غير موجود 404. لا يوجد مسار مقابل لعلامة غير مقروءة.

### DELETE /api/Admin/Contacts/{id}

Admin، بدون body؛ حذف فعلي. نجاح 200، غير موجود 404.

## 12. إدارة المستخدمين

كل المسارات التالية Admin.

### GET /api/Admin/Users

Query اختياري: `search` و`page=1`. البحث جزئي في userName والبريد وfname/lname، مع تجاهل الفراغات في بداية/نهاية عبارة البحث. حساسية حالة الأحرف تتبع collation قاعدة SQL Server؛ لا تفترض نفس النتيجة في كل بيئة.

العدد والصفحات يُحسبان بعد البحث. حجم الصفحة 5، وترتيب createdAt تنازلي ثم id تصاعدي. نجاح 200 مباشر:

```json
{
  "page": 1,
  "pageSize": 5,
  "totalPages": 1,
  "totalUsers": 1,
  "users": [
    {
      "id": "example-user-id",
      "userName": "ahmed.ali",
      "email": "student@example.com",
      "emailConfirmed": true,
      "role": "Student"
    }
  ]
}
```

role هنا أول دور أو null، وليس مصفوفة. إضافة بيانات جديدة قد تحرّك العناصر بين الصفحات؛ هذه Offset Pagination وليست لقطة ثابتة.

### GET /api/Admin/Users/{id}

200 مباشر:

```json
{
  "userName": "ahmed.ali",
  "email": "student@example.com",
  "emailConfirmed": true,
  "role": ["Student"]
}
```

role هنا **مصفوفة** خلاف القائمة. غير موجود 404. لا ترجع الاستجابة الأسماء المنفصلة ولا حالة القفل؛ هذه حدود يجب مراعاتها عند بناء شاشة تعديل المستخدم.

### POST /api/Admin/Users

JSON:

```json
{
  "firstName": "Ahmed",
  "lastName": "Ali",
  "userName": "managed.student",
  "email": "managed@example.com",
  "password": "Example@123",
  "role": "Student"
}
```

كل الحقول مطلوبة. الأسماء 2–50، اليوزر حتى 256 وبنفس الحروف المسموحة في Register، البريد صالح حتى 255، الباسورد 8–100 مع سياسة Identity، والدور Admin أو Student فقط.

انتبه: أسماء الحقول **firstName/lastName** هنا، و**fname/lname** في Register. الحساب المنشأ بواسطة الأدمن يُعد بريده مؤكدًا مباشرةً؛ لا يرسل بريد تأكيد في هذا المسار.

نجاح 201 مباشر، وليس الغلاف العام:

```json
{ "statusCode": 201, "message": "User created successfully", "userId": "example-user-id" }
```

فشل الإنشاء/إضافة الدور 400، وقد يحدث استثناء 500 في حالات غير معالجة. إنشاء الحساب والدور ليسا حاليًا Transaction واحدة؛ راجع الملاحظات الأخيرة قبل إعادة الطلب بعد فشل.

### PUT /api/Admin/Users/{id}

JSON، حقول اختيارية: `firstName`, `lastName`, `userName`, `email`, `password`, `role`. قيود الإنشاء تطبق عند إرسالها؛ كلمة مرور جديدة لا تحتاج confirmPassword في مسار الأدمن.

تغيير email لا يغيّر userName؛ تغيير userName لا يغيّر email. تغيير البريد يلغي تأكيده عبر Identity، فيحتاج المستخدم تأكيد البريد الجديد قبل Login. المسار لا يرسل بريد تأكيد تلقائيًا؛ يمكن استخدام ResendEmailConfirmation بمعرفه.

إرسال role يقصد به الدور النهائي، مع حذف الأدوار القديمة. نجاح 200 مباشر:

```json
{ "message": "User updated successfully", "userId": "example-user-id" }
```

غير موجود 404، فشل Identity 400. التعديل ما زال متعدد الخطوات بدون Transaction؛ بعض البيانات قد تُحفظ قبل فشل خطوة لاحقة.

### PATCH /api/Admin/Users/{id}/toggle-status

بدون body؛ يقلب حالة القفل. يفتح الحساب لو LockoutEnd في المستقبل، أو يقفله لمدة طويلة خلاف ذلك. نجاح 200 مباشر:

```json
{ "message": "User locked successfully", "isLocked": true }
```

أو isLocked=false عند الفتح. اعتمد على نتيجة العملية لتحديث زر الحالة. غير موجود 404، فشل Identity 400. لا يوجد حقل حالة القفل في GET القائمة/التفاصيل الحاليين. القفل يمنع Login، لكنه لا يلغي فورًا JWT صادرًا من قبل.

### DELETE /api/Admin/Users/{id}

بدون body؛ حذف فعلي للحساب عبر Identity. نجاح 200 مباشر:

```json
{ "message": "User deleted successfully", "userId": "example-user-id" }
```

غير موجود 404، فشل Identity 400. لا يوجد Restore. لا تفترض أن كل البيانات التابعة محفوظة بعد حذف المستخدم؛ قاعدة البيانات تتعامل مع العلاقات وفق قيودها.

## 13. لوحة التحكم

### GET /api/Admin/Dashboard

Admin، بدون body. نجاح 200 كائن مباشر يحتوي:

```json
{
  "usersCount": 10,
  "studentCount": 9,
  "instructorsCount": 3,
  "coursesCount": 4,
  "lastCourses": [],
  "lastUsers": [],
  "lastContactMessages": []
}
```

الأعداد توضيحية. instructorsCount/coursesCount يستبعدان المحذوفين. usersCount يشمل الحسابات الموجودة حتى لو مقفولة أو بريدها غير مؤكد.

- lastCourses: أحدث 3؛ الحقول `id,name,slug,price,description,isActive,isDeleted,createdAt,coverImageUrl,courseInstructors`، ولا تعتمد على تحميل courseInstructors.
- lastUsers: أحدث 3؛ `id,userName,email`.
- lastContactMessages: أحدث 3 كائنات ContactEntity.

## 14. استخدام الصور من الفرونت

المسموح للرفع PNG/JPG/JPEG فقط، ملف غير فارغ، بحد أقصى **5 MiB = 5 × 1024 × 1024 بايت**. القيود الحالية تفحص الامتداد والحجم، ولا تعني فحص محتوى الصورة أو تحويلها.

المسارات التي ترجعها القراءة:

```text
/img/courses_img/<filename>
/img/instructors_img/<filename>
/img/profiles/<filename>
```

الباك يقدّمها كملفات Static عامة بدون توكن. لا ترسل ملفًا يحتوي معلومات خاصة. أسماء الملفات القديمة تتحول لنفس المسارات، والروابط الخارجية تبقى كما هي.

```javascript
function resolveImageUrl(value, apiBaseUrl) {
  return value ? new URL(value, apiBaseUrl).href : null;
}
```

استخدم نفس Base URL الخاص بالـ API. عند null أو فشل تحميل الصورة اعرض placeholder. لا تضف `/img/` يدويًا إلى رابط خارجي، ولا تتعامل مع اسم الملف كأنه رابط الفرونت.

## 15. مثال عميل fetch

هذا مثال يوضح JSON وFormData واستخراج رسائل الأخطاء. اختيار تخزين التوكن وتنظيم الحالة يظل قرار الفرونت؛ لا يوجد refresh تلقائي في هذا المثال.

```javascript
const API_BASE_URL = 'https://localhost:7204';

async function apiRequest(path, { method = 'GET', body, token } = {}) {
  const headers = { Accept: 'application/json' };
  const isForm = body instanceof FormData;
  if (token) headers.Authorization = `Bearer ${token}`;
  if (body !== undefined && !isForm) headers['Content-Type'] = 'application/json';

  const response = await fetch(new URL(path, API_BASE_URL), {
    method,
    headers,
    body: body === undefined ? undefined : isForm ? body : JSON.stringify(body)
  });

  const text = await response.text();
  let result = null;
  try { result = text ? JSON.parse(text) : null; } catch { /* non-JSON error */ }

  if (!response.ok) {
    let messages = [];
    if (result?.errors) messages = Object.values(result.errors).flat();
    else if (Array.isArray(result?.message)) messages = result.message;
    else if (typeof result?.message === 'string') messages = [result.message];
    else if (Array.isArray(result)) messages = result.map(e => e.description).filter(Boolean);
    if (!messages.length) messages = [`Request failed (${response.status}).`];

    const error = new Error(messages.join('\n'));
    error.status = response.status;
    error.messages = messages;
    throw error;
  }
  return result;
}

const login = await apiRequest('/api/Identity/Accounts/Login', {
  method: 'POST',
  body: { userName: 'ahmed.ali', password: 'Example@123', rememberMe: false }
});
const token = login.data;
const profile = await apiRequest('/api/Profile', { token });

const query = new URLSearchParams({ search: 'ahmed', page: '1' });
// يحتاج هنا توكن Admin، وليس بالضرورة توكن الطالب المستخدم أعلاه.
// await apiRequest(`/api/Admin/Users?${query}`, { token: adminToken });
```

لا تمرر data تلقائيًا لكل الاستجابات في interceptor، وإلا ستفقد قوائم وبيانات ترجع مباشرةً. اجعل كل دالة API تعرف شكل المسار الذي تستخدمه.

## 16. خطوات استخدام Postman

1. Import لملف CodegateTest.postman_collection.json.
2. اضبط baseUrl، وبيانات حسابي الاختبار studentUserName/studentPassword وadminUserName/adminPassword.
3. نفّذ Login للطالب أو Login — Admin؛ السكربت يحفظ studentToken/adminToken تلقائيًا.
4. Requests المحمية تستخدم متغير التوكن المناسب تلقائيًا. البروفايل افتراضيًا يستخدم studentToken، ويمكن تغييره للأدمن.
5. اضبط courseId/instructorId/reviewId/contactId/managedUserId على معرفات موجودة، بدل افتراض أن الرقم 1 موجود.
6. في FormData اختر الملفات يدويًا، وعطّل الحقول الاختيارية غير المطلوبة. كرر InstructorIds لإضافة أكثر من مدرس.
7. التسجيل يحفظ accountUserId؛ Forget يحفظ المعرف المسترجع؛ Validate يحفظ resetToken. أدخل otp وconfirmationToken يدويًا من البريد.
8. collection تحتوي 41 مسارًا فريدًا، وطلب Login إضافيًا للأدمن. لا تعمل Run Collection بالكامل على بيانات حقيقية؛ فيها تعديل وحذف.

الباسوردات والتوكنات الحقيقية ليست مرفقة. لا تشارك نسخة collection بعد إدخال أسرار دون مسح المتغيرات.

## 17. حدود النسخة الحالية التي تؤثر على التسليم

هذه ملاحظات على الكود الحالي، وليست ميزات مطبقة أو تأكيدًا أن الفرونت مسؤول عن إصلاح الباك:

- بعض عمليات التقييمات والرسائل والحذف، وتعديلات بدون صورة، قد ترجع نجاحًا رغم فشل CommitAsync؛ الـ Repository يخفي استثناء الحفظ ويرجع 0، وبعض المسارات لا تفحصه. يحتاج إصلاحًا من الباك قبل الاعتماد على نجاح الكتابة.
- إنشاء المستخدم من الأدمن وتعديل بياناته/دوره لم يُحوّلا إلى Transaction. فشل خطوة قد يترك حسابًا بدون دور أو تغييرات جزئية. بعد فشل، أعد القراءة بدل تكرار الإنشاء تلقائيًا. التسجيل العام لديه تعويض بحذف الحساب عند فشل الدور، وليس Transaction كاملة.
- فشل إرسال بريد OTP غير متعالج حاليًا بعد حفظ الكود، وقد يرجع 500. بيانات SMTP داخل الكود تحتاج نقلًا لإعدادات سرية وتغيير بيانات الاعتماد قبل مشاركة المصدر؛ لا تُدرج الأسرار في وثائق الفرونت.
- تغيير البريد في إدارة المستخدمين يتطلب تأكيدًا جديدًا، ولا يرسل رسالة تلقائيًا.
- JWT لا يُلغى فورًا عند قفل الحساب أو تغيير الدور أو تغيير الباسورد. الصلاحيات داخل توكن قديم تظل حتى انتهاء مدته، ما لم تُضاف آلية تحقق/إلغاء من الباك.
- إنشاء الكورس والمدرس والتقييم والرسالة لا يرجع معرف الكيان الجديد. GET المستخدم لا يرجع أسماءه المنفصلة أو حالة قفله، وGET الكورس لا يرجع InstructorIds. هذه نواقص عقد مهمة لشاشات التعديل ويجب تنسيقها، لا تعويضها بتخمين المعرفات.
- لا يوجد مسار تقييمات الطالب الشخصية؛ لا يمكن بناء إدارة Pending/Rejected الخاصة به كاملةً من مسار Approved فقط.
- لا توجد مسارات محتوى دروس أو فيديوهات أو تسجيل في كورس أو دفع أو مشتريات أو progress. لا تفترض وجود هذه الميزات من اسم المشروع أو وجود قائمة كورسات.
- حقول التعديل الاختيارية null تعني الحفاظ على القيمة، وليست أمرًا لمسحها. JSON الفارغ في بعض طلبات التعديل قد يُقبل؛ لا تبنِ حفظًا بلا تغيير في الواجهة.

## 18. تجربة الربط قبل التسليم النهائي

اختبار عملي مشترك بين الباك والفرونت، باستخدام حسابات وبيانات اختبار:

- اتصال HTTPS وCORS من Origin الفرونت الفعلي.
- Register ثم بريد التأكيد ثم Login باليوزر، وفشل البريد وإعادة التأكيد.
- Forget ثم OTP ثم Reset ثم Login بالباسورد الجديد، ومحاولة OTP خاطئة/منتهية.
- اختبار 401 و403 وإعادة تسجيل الدخول عند انتهاء JWT.
- عرض الكورسات والمدرسين والصور، ورفع صورة جديدة وتحديثها.
- إنشاء كورس بمدرس موجود، رفض ID غير صالح أو مكرر، واختفاء العناصر المحذوفة.
- تقييم طالب ثم موافقة الأدمن، وظهوره ضمن Approved، وإعادة Pending بعد التعديل.
- إنشاء رسالة تواصل، قراءتها وتعليمها مقروءة بواسطة الأدمن.
- البحث والصفحات، وتأكيد عدد النتائج بعد البحث.
- مراجعة الحفظ الجزئي وفشل الحفظ مع مسؤول الباك قبل إغلاق ملاحظات التسليم.

الملفات المرافقة تمت مراجعتها مقابل تعريفات الكود، وليست تقرير نجاح للسيناريوهات الحية أعلاه. أي تغيير لاحق في المسارات أو الـ DTOs أو أشكال الاستجابات يستلزم تحديث الدليل وPostman معًا.
