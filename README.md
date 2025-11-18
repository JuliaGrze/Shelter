# Shelter – system do obsługi adopcji zwierząt 🐾

Shelter to pełna aplikacja webowa wspierająca schronisko w całym procesie adopcyjnym: od przeglądania zwierząt, przez składanie wniosków i wizyty domowe, aż po podpisywanie umów online.  
Dodatkowo system obejmuje **moduł zdrowia zwierząt** (karta medyczna) oraz **moduł darowizn** z integracją Stripe.

Aplikacja posiada panele dla ról: **Klient**, **Pracownik**, **Administrator**.

---

## 🚀 Technologie

**Backend:** ASP.NET Core, Entity Framework Core, ASP.NET Identity, Repository + Unit of Work, QuestPDF, Stripe  
**Frontend:** Angular, HttpClient, Reactive Forms, routing, interceptory, role  
**Baza danych:** SQL Server (migracje EF Core)  
**Architektura:** Domain → Application → Infrastructure → API → Frontend

---

## 🐾 Główne funkcje aplikacji

### 👤 Klient
- przeglądanie i filtrowanie zwierząt  
- szczegóły zwierzęcia + historia zdrowotna  
- formularz adopcyjny  
- panel „Moje wnioski”  
- podpisywanie umowy online (canvas → PDF)  
- możliwość dokonania **darowizny** online  

### 👷‍♀️ Pracownik
- pełny panel wniosków (filtry, wyszukiwanie, statusy)  
- szczegóły wniosku, notatki, historia zmian  
- planowanie wizyt domowych + zapis wyników  
- zarządzanie umowami adopcyjnymi  
- prowadzenie **kart zdrowia zwierząt**:
  - szczepienia  
  - zabiegi  
  - leczenia  
  - notatki weterynaryjne  

### 🛠 Administrator
- zarządzanie zwierzętami + ich statusem  
- wprowadzanie i edycja danych zdrowotnych  
- słowniki: gatunki, statusy adopcji, typy zabiegów  
- zarządzanie użytkownikami i rolami  
- dostęp do historii i listy **darowizn Stripe**  

---

## 🐶 Moduł zdrowia zwierząt

Każde zwierzę posiada **kartę medyczną**, na której pracownik może rejestrować:

- daty szczepień  
- podane leki  
- zabiegi (np. sterylizacja, leczenie, RTG)  
- notatki weterynaryjne  
- historię leczenia i przeprowadzonych badań  

Dzięki temu klient na stronie szczegółów zwierzęcia widzi jego **stan zdrowia** i najważniejsze informacje przed adopcją.

---

## 💛 Moduł darowizn (Stripe)

System posiada pełny moduł płatności wspierający działalność schroniska.

Funkcje:

- formularz darowizny (kwota, dane, opis)  
- obsługa płatności Stripe (PaymentIntent)  
- potwierdzenie udanej wpłaty  
- historia darowizn widoczna w panelu admina  
- walidacja danych i czytelne komunikaty o błędach  
- bezpieczna płatność i automatyczna aktualizacja statusu  

---

## 🔄 Proces adopcji

1. Klient wybiera zwierzę i wysyła wniosek  
2. Pracownik analizuje dane i planuje wizytę domową  
3. Po akceptacji generowana jest umowa PDF  
4. Klient podpisuje ją online (canvas → PDF)  
5. Zwierzę zostaje oznaczone jako adoptowane  

## 1. Publiczna część aplikacji

### Strona główna
<img width="1904" height="943" alt="Zrzut ekranu 2025-11-18 150354" src="https://github.com/user-attachments/assets/b1fba18e-47e9-4c8d-899b-191bfc627b22" />

### Filtrowanie zwierząt
<img width="1392" height="666" alt="Zrzut ekranu 2025-11-18 150409" src="https://github.com/user-attachments/assets/be006400-1b7e-4f8b-aad7-782a56a2800a" />

---

## 2. Szczegóły zwierzęcia + wniosek adopcyjny

### Szczegóły zwierzęcia
<img width="1316" height="714" alt="Zrzut ekranu 2025-11-18 150527" src="https://github.com/user-attachments/assets/19565ebd-1060-433d-baa1-c8ebd190c4c7" />

### Formularz adopcyjny
<img width="614" height="267" alt="image" src="https://github.com/user-attachments/assets/7caa34a1-87b7-4aa4-8a70-2b1a8e972b5a" />

---

## 3. Panel klienta

### Moje wnioski
<img width="1290" height="297" alt="Zrzut ekranu 2025-11-18 152014" src="https://github.com/user-attachments/assets/759dc10d-4c79-498e-a2bb-583b040b667f" />

### Szczegóły wniosku klienta
<img width="1071" height="614" alt="Zrzut ekranu 2025-11-18 152036" src="https://github.com/user-attachments/assets/d87fef84-623f-41cf-bc9d-8afc7e530572" />
<img width="1092" height="443" alt="Zrzut ekranu 2025-11-18 152029" src="https://github.com/user-attachments/assets/81c0fa7e-fe91-4544-937a-07863fe78ce1" />

### Podpisywanie umowy
<img width="1213" height="556" alt="Zrzut ekranu 2025-11-18 151437" src="https://github.com/user-attachments/assets/fd6f9037-6524-4c3b-b48b-6ea6298a64f6" />

### Podgląd PDF umowy
<img width="687" height="741" alt="Zrzut ekranu 2025-11-18 152101" src="https://github.com/user-attachments/assets/da3713b2-bc46-42d7-8b17-f2ebf5e34367" />
---

## 4. Panel pracownika

### Lista wniosków
<img width="1314" height="563" alt="Zrzut ekranu 2025-11-18 151806" src="https://github.com/user-attachments/assets/dcc69a77-a1ba-44b5-aeea-5224bf3e0b56" />

### Szczegóły wniosku (widok pracownika)
<img width="862" height="936" alt="Zrzut ekranu 2025-11-18 151930" src="https://github.com/user-attachments/assets/fccfe909-7e50-44be-a4b6-db049420971a" />

### Dodawanie wizyty domowej
<img width="875" height="669" alt="Zrzut ekranu 2025-11-18 151935" src="https://github.com/user-attachments/assets/8e08898b-181d-4bcd-b852-5ca1d37dd8cf" />


### Obsługa umowy
<img width="364" height="137" alt="image" src="https://github.com/user-attachments/assets/29ad5bb3-86e2-4278-9f2d-bd106d0a6c1a" />
<img width="267" height="143" alt="image" src="https://github.com/user-attachments/assets/186e687a-124a-458e-a3eb-a7790b71879f" />

---

## 5. Panel administratora

### Lista zwierząt (admin)
<img width="1902" height="942" alt="Zrzut ekranu 2025-11-18 151637" src="https://github.com/user-attachments/assets/793972b3-b21d-4daa-bd78-40d787485641" />

### Panel zarządzania
<img width="670" height="270" alt="Zrzut ekranu 2025-11-18 151709" src="https://github.com/user-attachments/assets/c4ef7e29-919c-41c7-8628-1d53a8cb1228" />

### Edycja / dodawanie zwierzaka
<img width="836" height="932" alt="Zrzut ekranu 2025-11-18 151659" src="https://github.com/user-attachments/assets/32bada87-a2cd-46f5-a258-dd3300f95b21" />
<img width="878" height="507" alt="Zrzut ekranu 2025-11-18 151714" src="https://github.com/user-attachments/assets/a308b553-48d1-4452-aba3-b25f14bca661" />

### Dodawanie gatunku
<img width="447" height="301" alt="Zrzut ekranu 2025-11-18 151726" src="https://github.com/user-attachments/assets/c4bb81ad-0366-47c5-b8c5-b7df62dcdb0f" />

### Zarządzanie gatunkami
<img width="771" height="852" alt="Zrzut ekranu 2025-11-18 151734" src="https://github.com/user-attachments/assets/6e12e155-d17e-4c7b-a4c3-ea0f1def8bac" />

---

## 6. Moduł darowizn 

### Formularz darowizny
<img width="589" height="583" alt="Zrzut ekranu 2025-11-18 150440" src="https://github.com/user-attachments/assets/a0057395-2c3a-4ad5-a56a-ab9cb7eb220c" />

### Potwierdzenie płatności
<img width="902" height="559" alt="Zrzut ekranu 2025-11-18 150505" src="https://github.com/user-attachments/assets/a18432d1-61f7-4246-a87d-617004c36147" />

### Widok wpłaconych darowizn
<img width="865" height="940" alt="Zrzut ekranu 2025-11-18 150426" src="https://github.com/user-attachments/assets/9bb32a59-68b5-4d8e-a132-44ca60c7966c" />

---

