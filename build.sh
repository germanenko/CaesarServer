# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Массив сервисов
SERVICES=(
    "planner-auth-service"
    "planner-chat-service"
    "planner-content-service"
    "planner-node-service"
    "planner-notify-service"
    "planner-file-service"
    "planner-email-service"
    "planner-mailbox-service"
)

echo -e "${GREEN}=== Начало сборки всех сервисов ===${NC}"
echo ""

for service in "${SERVICES[@]}"; do
    echo -e "${YELLOW}=== Сборка $service ===${NC}"
    
    # Проверяем наличие dockerfile в директории сервиса
    if [ -f "$service/dockerfile" ]; then
        # Собираем образ используя dockerfile из директории сервиса и контекст из родительской директории
        docker build -f "$service/dockerfile" -t "$service:latest" ..
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}✓ $service успешно собран${NC}"
        else
            echo -e "${RED}✗ Ошибка при сборке $service${NC}"
        fi
    else
        echo -e "${RED}Ошибка: dockerfile не найден в $service/${NC}"
    fi
    
    echo ""
done

echo -e "${GREEN}=== Сборка всех сервисов завершена ===${NC}"