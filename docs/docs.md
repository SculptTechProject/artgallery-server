# ArtGallery API Documentation (v1)

RESTful API for managing artists, artworks, and uploads in the **ArtGallery** system.

---

## 🔐 Admin Authentication

### POST `/api/v1/admin/login`

Authenticate admin and receive JWT token.

**Request Body**:

```json
{
  "username": "admin",
  "password": "secret123"
}
```

**Response 200 OK**:

```json
{
  "token": "<jwt_token>"
}
```

**Errors**:

* `401 Unauthorized` – invalid credentials

Token is valid for **2 hours**.

---

## 👩‍🎨 Artists

### POST `/api/v1/artists`

Create a new artist.

**Request Body**:

```json
{
  "name": "Zofia",
  "surname": "Nowak",
  "biography": "Abstrakcjonistka z Krakowa."
}
```

**Response 200 OK**:

```json
{
  "id": "11111111-2222-3333-4444-555555555555",
  "name": "Zofia",
  "surname": "Nowak",
  "biography": "Abstrakcjonistka z Krakowa."
}
```

**Errors**:

* `400 BadRequest` – missing name or surname

---

### GET `/api/v1/artists`

Retrieve all artists.

**Response 200 OK**:

```json
[
  {
    "id": "...",
    "name": "Zofia",
    "surname": "Nowak",
    "biography": "Abstrakcjonistka z Krakowa."
  }
]
```

---

### GET `/api/v1/artists/{id}?expandArts=true`

Retrieve a single artist with optional list of artworks.

**Example**:

```
GET /api/v1/artists/1111-2222-3333-4444-5555?expandArts=true
```

**Response 200 OK**:

```json
{
  "id": "...",
  "name": "Zofia",
  "surname": "Nowak",
  "biography": "...",
  "arts": [
    { "id": "...", "title": "Mgla nad Wisla", "description": "...", "type": "Painting" }
  ]
}
```

---

### GET `/api/v1/artists/{id}/grouped`

Retrieve artist with artworks grouped by type.

**Response 200 OK**:

```json
{
  "id": "...",
  "name": "Zofia",
  "surname": "Nowak",
  "biography": "...",
  "artsByType": {
    "Painting": [ { "id": "...", "title": "..." } ],
    "Sculpture": [ { "id": "...", "title": "..." } ]
  }
}
```

---

### PATCH `/api/v1/artists/{id}`

Partially update artist information.

**Request Body**:

```json
{ "biography": "Nowy opis" }
```

**Response 200 OK**:

```json
{
  "id": "...",
  "name": "Zofia",
  "surname": "Nowak",
  "biography": "Nowy opis"
}
```

---

### DELETE `/api/v1/artists/{id}`

Delete artist (only if they have no artworks).

**Responses**:

* `204 NoContent` – deleted
* `404 NotFound` – artist not found
* `409 Conflict` – artist has linked artworks

---

## 🖼️ Artworks

### POST `/api/v1/arts`

Add a new artwork.

**Request Body**:

```json
{
  "title": "Mgla nad Wisla",
  "description": "Akryl na plótnie, 80x60 cm.",
  "artistId": "11111111-2222-3333-4444-555555555555",
  "imageUrl": "/images/artworks/mgla.jpg",
  "type": "Painting"
}
```

**Response 201 Created**:

```json
{
  "id": "...",
  "title": "Mgla nad Wisla",
  "description": "Akryl na plótnie, 80x60 cm.",
  "imageUrl": "/images/artworks/mgla.jpg",
  "artist": { "id": "...", "name": "Zofia", "surname": "Nowak", "biography": "..." },
  "type": "Painting"
}
```

**Errors**:

* `400 BadRequest` – title or description missing
* `404 NotFound` – artist not found

---

### GET `/api/v1/arts`

List artworks with optional filters.

**Query Parameters**:

| Parameter  | Type      | Description                      |
| ---------- | --------- | -------------------------------- |
| `type`     | `ArtType` | filter by type                   |
| `search`   | string    | text search in title/description |
| `page`     | int       | default 1                        |
| `pageSize` | int       | max 100                          |

**Response 200 OK**:

```json
[
  { "id": "...", "title": "Mgla nad Wisla", "description": "...", "type": "Painting" }
]
```

Header: `X-Total-Count: 32`

---

### GET `/api/v1/arts/{id}`

Get a single artwork with its artist.

---

### GET `/api/v1/arts/random?limit=10&type=Photography`

Retrieve random artworks (optionally filtered by type).

---

### GET `/api/v1/arts/type/{type}`

List artworks of a specific type.

---

### GET `/api/v1/arts/categories`

Retrieve all existing categories (ArtTypes) present in the database.

**Query Parameters**:

| Parameter        | Type | Description                             |
| ---------------- | ---- | --------------------------------------- |
| `includeUnknown` | bool | include `Unknown` type                  |
| `minCount`       | int  | minimum number of artworks per category |

**Response 200 OK**:

```json
[
  { "id": 1, "name": "Malarstwo", "slug": "painting", "count": 12 },
  { "id": 2, "name": "Rzeźba", "slug": "sculpture", "count": 5 }
]
```

---

## 📤 Uploads

### POST `/api/v1/uploads/arts`

Upload an image file for artwork.

**Form-data**:

| Field  | Type | Description                                   |
| ------ | ---- | --------------------------------------------- |
| `file` | File | image file (`.jpg`, `.png`, `.webp`, `.avif`) |

**Response 200 OK**:

```json
"/images/artworks/9b83f72b91b84b0796d6eccc5f42c657.webp"
```

**Errors**:

* `400 BadRequest` – empty or invalid file type
* Size limit: 10MB

---

## 🎨 Enum `ArtType`

| Value          | Description       |
| -------------- | ----------------- |
| `Unknown`      | Unknown type      |
| `Painting`     | Malarstwo         |
| `Drawing`      | Rysunek           |
| `Sculpture`    | Rzeźba            |
| `Photography`  | Fotografia        |
| `Digital`      | Sztuka cyfrowa    |
| `MixedMedia`   | Technika mieszana |
| `Video`        | Wideo             |
| `Textile`      | Tkanina           |
| `Ceramic`      | Ceramika          |
| `Glass`        | Szkło             |
| `Sound`        | Dźwięk            |
| `Street`       | Street art        |
| `Illustration` | Ilustracja        |

---

## 🧱 Technical Notes

* All requests use `application/json` unless stated otherwise.
* Pagination returns `X-Total-Count` header.
* File uploads are stored in `wwwroot/images/artworks`.
* Admin login issues JWT token (2h expiry).
* Authenticated endpoints (future) should include header: `Authorization: Bearer <token>`.

---

**Base URL:** `http://localhost:5000`

---
