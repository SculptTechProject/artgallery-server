# Art API – Dokumentacja (v1)

Dokument opisuje zasoby **Art** w usłudze ArtGallery. Zawiera formaty danych, punkty końcowe, parametry, statusy odpowiedzi oraz przykłady.

---

## 1. Modele i DTO

### 1.1. Enum `ArtType`

* `Painting`
* `Sculpture`
* `Photography`
* `Drawing`
  *(lista przykładowa — dopasuj do swojej aplikacji)*

### 1.2. `ArtDto`

```json
{
  "id": "uuid",
  "title": "string",
  "description": "string",
  "artist": {
    "id": "uuid",
    "name": "string",
    "surname": "string",
    "biography": "string"
  },
  "type": "ArtType"
}
```

### 1.3. `CreateArtDto`

```json
{
  "title": "string (required)",
  "description": "string (required)",
  "artistId": "uuid (required)",
  "type": "ArtType (required)"
}
```

### 1.4. `ArtMiniDto` (używany w zagnieżdżeniach przy artyście)

```json
{
  "id": "uuid",
  "title": "string",
  "description": "string",
  "type": "ArtType"
}
```

---

## 2. Punkty końcowe

### 2.1. Utwórz pracę (Art)

**POST** `/api/v1/art`

**Body** (`CreateArtDto`):

```json
{
  "title": "Mgła nad Wisłą",
  "description": "Akryl na płótnie, 80x60 cm.",
  "artistId": "11111111-2222-3333-4444-555555555555",
  "type": "Painting"
}
```

**Odpowiedzi**:

* `201 Created` — zwraca `ArtDto` + nagłówek `Location: /api/v1/art/{id}`
* `400 BadRequest` — brak wymaganych pól (`title`, `description`)
* `404 NotFound` — nieistniejący `artistId`

**Przykład cURL**

```bash
curl -X POST http://localhost:8080/api/v1/art \
  -H "Content-Type: application/json" \
  -d '{
    "title":"Mgła nad Wisłą",
    "description":"Akryl na płótnie, 80x60 cm.",
    "artistId":"11111111-2222-3333-4444-555555555555",
    "type":"Painting"
  }'
```

---

### 2.2. Pobierz pracę po Id

**GET** `/api/v1/art/{id}`

**Parametry**:

* `id` — `uuid` (route)

**Odpowiedzi**:

* `200 OK` — `ArtDto`
* `404 NotFound`

**Przykład**

```bash
curl http://localhost:8080/api/v1/art/77777777-8888-9999-aaaa-bbbbbbbbbbbb
```

---

### 2.3. Lista prac z filtrowaniem i paginacją

**GET** `/api/v1/art`

**Query parametry**:

* `type` — `ArtType` *(opcjonalne; filtr po typie)*
* `search` — `string` *(opcjonalne; tytuł/description Contains)*
* `page` — `int` *(domyślnie 1, min 1)*
* `pageSize` — `int` *(domyślnie 20, min 1, max 100)*

**Odpowiedź**:

* `200 OK` — `ArtDto[]`
* Nagłówek `X-Total-Count` — całkowita liczba pasujących rekordów

**Przykłady**

```bash
# strona 1, 20 rekordów
curl "http://localhost:8080/api/v1/art?page=1&pageSize=20"

# filtr po typie
curl "http://localhost:8080/api/v1/art?type=Painting"

# filtr tekstowy (tytuł/description zawiera 'wisla')
curl "http://localhost:8080/api/v1/art?search=wisla"
```

**Uwagi**:

* Sortowanie domyślne: `ORDER BY Id DESC` (do zmiany wg potrzeb)
* Pole `search` używa `Contains` (SQLite: `LIKE` case-sensitive). Dla FTS rozważ migrację do wyszukiwarki pełnotekstowej.

---

### 2.4. Losowe prace (opcjonalnie po typie)

**GET** `/api/v1/art/random`

**Query parametry**:

* `limit` — `int` *(domyślnie 20, 1–100)*
* `type` — `ArtType` *(opcjonalnie)*

**Odpowiedzi**:

* `200 OK` — `ArtDto[]`

**Przykład**

```bash
curl "http://localhost:8080/api/v1/art/random?limit=20&type=Painting"
```

**Uwagi wydajnościowe**:

* SQLite/EF: `ORDER BY random()` tasuje całą tabelę. Dla małych/średnich zbiorów OK. Dla dużych — rozważ losowanie po kluczach lub cache.

---

## 3. Walidacja i błędy

### 3.1. Reguły minimalne

* `title` — required, `Trim()`
* `description` — required, `Trim()`
* `artistId` — musi wskazywać istniejącego artystę (`Artists.Any(id)`).
* `type` — poprawna wartość z `ArtType`.

### 3.2. Kody odpowiedzi

* `200 OK` — zwrot danych
* `201 Created` — utworzono zasób
* `400 BadRequest` — brak/nieprawidłowe dane wejściowe
* `404 NotFound` — brak zasobu/relacji (np. nieistniejący `artistId`)

**Format błędu (przykład)**

```json
{
  "error": "Title and description are required"
}
```

---

## 4. Konwencje i nagłówki

* `Content-Type: application/json`
* `Accept: application/json`
* Przy `POST /api/v1/art` — `Location: /api/v1/art/{id}`
* Paginacja — `X-Total-Count`

---

## 5. Bezpieczeństwo (opcjonalnie)

Jeśli endpointy są chronione JWT:

* Schemat: `Authorization: Bearer <token>`
* Swagger: zdefiniowany `Bearer` i `SecurityRequirement`

---

## 6. Przykładowe odpowiedzi (Sample)

### 6.1. `ArtDto`

```json
{
  "id": "7ae4a2f1-2a8b-4e0f-9f6b-1f7e3a1b2c3d",
  "title": "Mgła nad Wisłą",
  "description": "Akryl na płótnie, 80x60 cm.",
  "artist": {
    "id": "1e0f7f9a-0f0d-4b34-8a07-9b5d1b3e9a55",
    "name": "Zofia",
    "surname": "Nowak",
    "biography": "Abstrakcjonistka z Krakowa."
  },
  "type": "Painting"
}
```

### 6.2. Lista z `X-Total-Count`

Header:

```
X-Total-Count: 128
```

Body:

```json
[
  {
    "id": "...",
    "title": "...",
    "description": "...",
    "artist": { "id": "...", "name": "...", "surname": "...", "biography": "..." },
    "type": "Painting"
  }
]
```

---

## 7. Dobre praktyki

* Zwracaj `201 Created` + `Location` przy tworzeniu.
* Unikaj przekazywania pełnego `ArtistDto` jako wejścia — używaj `artistId`.
* Projekcje `.Select(...)` bez `Include` — EF wykona JOIN i zwróci tylko potrzebne pola.
* Ustal limity `pageSize` i `limit` (np. max 100) – ochronisz bazę.
* Dodaj indeks po `Arts(ArtistId)` i ewentualnie po `Arts(Type)`.

---

## 8. Swagger / UI

* UI: `/docs`
* JSON: `/swagger/v1/swagger.json` oraz `/openapi/v1.json`
* Dodaj komentarze XML przy akcjach, aby wzbogacić opisy i przykłady w UI.
