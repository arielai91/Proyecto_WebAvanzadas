#!/usr/bin/env bash
# ============================================================================
# seed-test-data.sh
# Uploads pet images from data/ to Supabase and inserts test pets into PostgreSQL.
#
# Usage:
#   chmod +x scripts/seed-test-data.sh
#   ./scripts/seed-test-data.sh
#
# Prerequisites:
#   - PostgreSQL running (make infra-up)
#   - curl and psql available
#   - .env or appsettings.json with Supabase config
# ============================================================================

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DATA_DIR="$PROJECT_ROOT/data"

# --- Configuration (from appsettings.json / .env) ---
SUPABASE_URL="${SUPABASE_URL:-https://xkyicdwuczjmcpzxejje.supabase.co}"
SUPABASE_KEY="${SUPABASE_KEY:-eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhreWljZHd1Y3pqbWNwenhlamplIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc2ODQwOTU1OSwiZXhwIjoyMDgzOTg1NTU5fQ.f2XyschLx1uEkJdGLbD5yzpwzSeSZLq23HIb06RTwzc}"
SUPABASE_BUCKET="${SUPABASE_BUCKET:-Pet}"

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-petfoundationdb}"
DB_USER="${DB_USER:-postgres}"
DB_PASS="${DB_PASS:-postgreSQL2025}"

export PGPASSWORD="$DB_PASS"

# --- Helper: upload image to Supabase ---
upload_image() {
    local file_path="$1"
    local pet_id="$2"
    local file_name
    file_name="$(basename "$file_path")"
    local uuid
    uuid="$(uuidgen | tr -d '-')"
    local remote_path="pets/${pet_id}/${uuid}_${file_name}"

    local upload_url="${SUPABASE_URL}/storage/v1/object/${SUPABASE_BUCKET}/${remote_path}?upsert=true"

    local tmp_response
    tmp_response=$(mktemp)
    local tmp_stderr
    tmp_stderr=$(mktemp)

    local http_code
    http_code=$(curl -s -o "$tmp_response" -w "%{http_code}" \
        --max-time 120 \
        --connect-timeout 15 \
        -X POST "$upload_url" \
        -H "Authorization: Bearer ${SUPABASE_KEY}" \
        -H "apikey: ${SUPABASE_KEY}" \
        -H "Content-Type: image/jpeg" \
        --data-binary "@${file_path}" \
        2>"$tmp_stderr") || true

    if [[ "$http_code" -ge 200 && "$http_code" -lt 300 ]]; then
        rm -f "$tmp_response" "$tmp_stderr"
        echo "${SUPABASE_URL}/storage/v1/object/public/${SUPABASE_BUCKET}/${remote_path}"
    else
        echo "ERROR: Upload failed for ${file_name} (HTTP ${http_code})" >&2
        echo "  Response: $(cat "$tmp_response")" >&2
        echo "  Stderr: $(cat "$tmp_stderr")" >&2
        rm -f "$tmp_response" "$tmp_stderr"
        return 1
    fi
}

# --- Get the admin user ID (CreatedById) ---
echo "==> Looking up admin user ID..."
ADMIN_ID=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -qtAc \
    "SELECT \"Id\" FROM app.\"Users\" WHERE \"Name\" = 'Administrator' LIMIT 1;" | head -1 | tr -d '[:space:]')

if [[ -z "$ADMIN_ID" ]]; then
    echo "ERROR: Admin user not found in app.Users. Run the backend first to seed users." >&2
    exit 1
fi
echo "    Admin user ID: $ADMIN_ID"

# --- Pet test data ---
# Each entry: "Name|Species|Breed|Age|Sex|Size|Description|ImageFile"
PETS=(
    "Luna|Perro|Labrador Retriever|3|Hembra|Grande|Luna es una labradora juguetona y cariñosa que ama correr en el parque. Es excelente con niños y otros animales. Está vacunada y esterilizada.|pexels-bertellifotografia-16652416.jpg"
    "Rocky|Perro|Pastor Alemán|5|Macho|Grande|Rocky es un pastor alemán leal y protector. Ideal para una familia activa con espacio al aire libre. Entrenado en obediencia básica.|pexels-ivan-s-6291567.jpg"
    "Milo|Perro|Beagle|2|Macho|Mediano|Milo es un beagle curioso y enérgico. Le encanta explorar y jugar con pelotas. Perfecto para un hogar con jardín.|pexels-lstan-1751542.jpg"
    "Bella|Perro|Golden Retriever|4|Hembra|Grande|Bella es una golden retriever dulce y tranquila. Le encanta recibir caricias y es muy obediente. Ideal como compañera de terapia.|pexels-maksgelatin-9956243.jpg"
    "Max|Perro|Bulldog Francés|1|Macho|Pequeño|Max es un bulldog francés travieso y lleno de energía. Perfecto para apartamentos. Le encanta dormir en el sofá después de jugar.|pexels-pixabay-97082.jpg"
    "Canela|Perro|Mestizo|3|Hembra|Mediano|Canela fue rescatada de la calle y ahora busca un hogar definitivo. Es tímida al principio pero muy leal cuando toma confianza.|pexels-rdne-7516850.jpg"
    "Thor|Perro|Husky Siberiano|2|Macho|Grande|Thor es un husky siberiano con ojos azules impresionantes. Necesita mucho ejercicio y un dueño experimentado. Le encanta el frío.|pexels-sam-lion-5732461.jpg"
    "Princesa|Perro|Poodle|6|Hembra|Pequeño|Princesa es una poodle elegante y bien educada. Ideal para personas mayores o familias tranquilas. No suelta mucho pelo.|pexels-thijsvdw-998254.jpg"
)

echo "==> Inserting ${#PETS[@]} test pets..."
echo ""

SUCCESS_COUNT=0

for i in "${!PETS[@]}"; do
    IFS='|' read -r name species breed age sex size description image_file <<< "${PETS[$i]}"

    echo "--- Pet $((i+1)): $name ($breed) ---"

    # Escape single quotes for SQL
    sql_name="${name//\'/\'\'}"
    sql_species="${species//\'/\'\'}"
    sql_breed="${breed//\'/\'\'}"
    sql_sex="${sex//\'/\'\'}"
    sql_size="${size//\'/\'\'}"
    sql_description="${description//\'/\'\'}"

    # Insert pet and get its ID
    PET_ID=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -qtAc \
        "INSERT INTO app.\"Pets\" (\"Name\", \"Species\", \"Breed\", \"Age\", \"Sex\", \"Size\", \"Description\", \"Status\", \"CreatedById\", \"CreatedAt\", \"UpdatedAt\")
         VALUES ('$sql_name', '$sql_species', '$sql_breed', $age, '$sql_sex', '$sql_size', '$sql_description', 'Available', $ADMIN_ID, NOW(), NOW())
         RETURNING \"Id\";" | head -1 | tr -d '[:space:]')

    if [[ -z "$PET_ID" ]]; then
        echo "    ERROR: Failed to insert pet, skipping." >&2
        continue
    fi

    echo "    Inserted pet ID: $PET_ID"

    # Upload image to Supabase
    echo "    Uploading image: $image_file"
    IMAGE_URL=$(upload_image "$DATA_DIR/$image_file" "$PET_ID")

    if [[ -z "$IMAGE_URL" || "$IMAGE_URL" == ERROR* ]]; then
        echo "    WARNING: Image upload failed, pet inserted without image." >&2
        echo ""
        continue
    fi

    echo "    Image URL: $IMAGE_URL"

    # Insert pet image record
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -qc \
        "INSERT INTO app.\"PetImages\" (\"PetId\", \"Url\", \"IsCover\", \"UploadedAt\")
         VALUES ($PET_ID, '$IMAGE_URL', true, NOW());"

    echo "    Image record inserted (IsCover=true)"
    SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
    echo ""
done

echo "==> Done! ${SUCCESS_COUNT}/${#PETS[@]} pets with images have been seeded."
echo ""
echo "Summary:"
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c \
    "SELECT p.\"Id\", p.\"Name\", p.\"Breed\", p.\"Sex\", p.\"Size\", p.\"Status\",
            (SELECT COUNT(*) FROM app.\"PetImages\" pi WHERE pi.\"PetId\" = p.\"Id\") as images
     FROM app.\"Pets\" p
     ORDER BY p.\"Id\";"
