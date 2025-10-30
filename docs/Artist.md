# Artist API – Dokumentacja (v1)

Dokument opisuje zasoby **Artist** w usłudze ArtGallery. Zawiera modele danych, punkty końcowe, parametry, statusy odpowiedzi oraz przykłady użycia.

---

## 1. Modele i DTO

### 1.1. `ArtistDto`

```json
{
  "id": "uuid",
  "name": "string",
  "surname": "string",
  "biography": "string"
}
```

### 1.2. `CreateArtistDto`

```json
{
  "name": "string (required)",
  "surname": "string (required)",
  "biography": "string (optional)"
}
```

### 1.3. `PatchArtistDto`

```json
{
  "name": "string or null (optional)",
  "surname": "string or null (optional)",
  "biography": "string or null (optional)"
}
```

### 1.4. `ArtistWithArtsDto` (rozszerzony widok z pracami)

```json
{
  "id": "uuid",
  "name": "string",
  "surname": "string",
  "biography": "string",
  "arts": [
    {
      "id": "uuid",
      "title": "string",
      "description": "string",
      "type": "ArtType"
    }
  ]
}
```

> Dla elementów w `arts` używany jest `ArtMiniDto` (id, title, description, type).

### 1.5. Enum `ArtType`

* `Painting`
* `Sculpture`
* `Photography`
* `Drawing`
  *(lista przykładowa — dopasuj do swojej aplikacji)*

---

## 2. Punkty końcowe

### 2.1. Utwórz artystę

**POST** `/api/v1/artist`

**Body** (`CreateArtistDto`):

```json
{
  "name": "Zofia",
  "surname": "Nowak",
  "biography": "Abstrakcjonistka z Krakowa."
}
```

**Odpowiedzi**:

* `201 Created` — zwraca `ArtistDto` + nagłówek `Location: /api/v1/artist/{id}`
* `400 BadRequest` — brak `name`/`surname`

**Przykład cURL**

```bash
curl -X POST http://localhost:8080/api/v1/artist \
  -H "Content-Type: application/json" \
  -d '{
    "name":"Zofia",
    "surname":"Nowak",
    "biography":"Abstrakcjonistka z Krakowa."
  }'
```

---

### 2.2. Pobierz listę artystów

**GET** `/api/v1/artist?includeCounts=true`

**Query parametry**:

* `includeCounts` — `bool` *(opcjonalne; domyślnie true)* — jeśli true, do każdego rekordu dołączana jest liczba prac (`artsCount`).

**Odpowiedzi**:

* `200 OK` — tablica `ArtistDto` lub obiektów `{ id, name, surname, biography, artsCount }`.

**Przykłady**

```bash
# bez liczników
curl "http://localhost:8080/api/v1/artist?includeCounts=false"

# z licznikami
curl "http://localhost:8080/api/v1/artist?includeCounts=true"
```

---

### 2.3. Pobierz artystę po Id (płytko)

**GET** `/api/v1/artist/{id}`

**Parametry**:

* `id` — `uuid` (route)
* `expandArts` — `bool` *(opcjonalny; domyślnie false)* — jeśli true, zwróci `ArtistWithArtsDto`.

**Odpowiedzi**:

* `200 OK` — `ArtistDto` lub `ArtistWithArtsDto`
* `404 NotFound`

**Przykłady**

```bash
# płytko
curl http://localhost:8080/api/v1/artist/11111111-2222-3333-4444-555555555555

# z pracami
curl "http://localhost:8080/api/v1/artist/11111111-2222-3333-4444-555555555555?expandArts=true"
```

---

### 2.4. Pobierz artystę z pracami pogrupowanymi po typie

**GET** `/api/v1/artist/{id}/grouped`

**Odpowiedzi**:

* `200 OK` — `{ id, name, surname, biography, artsByType: { "Painting": ArtMiniDto[], ... } }`
* `404 NotFound`

**Przykład**

```bash
curl http://localhost:8080/api/v1/artist/11111111-2222-3333-4444-555555555555/grouped
```

---

### 2.5. Zaktualizuj częściowo artystę

**PATCH** `/api/v1/artist/{id}`

**Body** (`PatchArtistDto`):

```json
{
  "name": "Zofia (aktualizacja)",
  "biography": "Nowy opis"
}
```

**Odpowiedzi**:

* `200 OK` — `ArtistDto`
* `404 NotFound`
* `400 BadRequest` — jeśli walidacja nie przejdzie

**Przykład cURL**

```bash
curl -X PATCH http://localhost:8080/api/v1/artist/11111111-2222-3333-4444-555555555555 \
  -H "Content-Type: application/json" \
  -d '{"biography":"Nowy bio"}'
```

---

### 2.6. Usuń artystę

**DELETE** `/api/v1/artist/{id}`

**Zasady**:

* Gdy artysta ma przypisane prace (`Arts`), zwracane jest `409 Conflict`.

**Odpowiedzi**:

* `204 NoContent` — usunięto
* `404 NotFound` — nie znaleziono
* `409 Conflict` — artysta ma powiązane prace

**Przykład cURL**

```bash
curl -X DELETE http://localhost:8080/api/v1/artist/11111111-2222-3333-4444-555555555555
```

---

## 3. Walidacja i kody błędów

### 3.1. Minimalne reguły

* `name` i `surname` — wymagane (Create)
* `Patch` — pola opcjonalne, aktualizowane tylko jeśli przesłane

### 3.2. Kody odpowiedzi

* `200 OK` — zwrot danych
* `201 Created` — utworzono zasób
* `204 NoContent` — usunięto
* `400 BadRequest` — niepoprawne dane wejściowe
* `404 NotFound` — brak zasobu
* `409 Conflict` — kolizja reguł (np. powiązane prace)

**Format błędu (przykład)**

```json
{
  "error": "Cannot delete artist with arts."
}
```

---

## 4. Dobre praktyki

* `CreatedAtRoute`/`CreatedAtAction` przy tworzeniu (nagłówek `Location`).
* Projekcje Linq `.Select(...)` zamiast `Include` w read-modelach.
* Limituj zakres PATCH do pól, które mogą być zmieniane.
* Indeks po `Arts(ArtistId)` (domyślnie EF tworzy FK index).

---

## 5. Swagger / UI

* UI: `/docs`
* JSON: `/swagger/v1/swagger.json` oraz `/openapi/v1.json`
* Uzupełnij komentarze XML przy akcjach, aby w Swaggerze były opisy i przykłady.
