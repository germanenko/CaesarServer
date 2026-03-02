#!/bin/bash

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Функция для проверки наличия необходимых файлов в родительской директории
check_prerequisites() {
    echo -e "${YELLOW}Проверка необходимых файлов в родительской директории...${NC}"
    
    # Проверяем наличие NuGet.config
    if [ ! -f "NuGet.config" ]; then
        echo -e "${YELLOW}NuGet.config не найден, создаю базовый...${NC}"
        cat > NuGet.config << 'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
EOF
    fi
    
    # Проверяем наличие nuget-local директории
    if [ ! -d "nuget-local" ]; then
        echo -e "${YELLOW}Директория nuget-local не найдена, создаю...${NC}"
        mkdir -p nuget-local
    fi
    
    # Проверяем наличие сертификата для auth-service
    if [ ! -f "planner-auth-service/local.pfx" ] && [ -d "planner-auth-service" ]; then
        echo -e "${YELLOW}Сертификат для auth-service не найден, создаю заглушку...${NC}"
        cd planner-auth-service
        mkdir -p https
        openssl req -x509 -newkey rsa:4096 \
            -keyout https/local.key \
            -out https/local.crt \
            -days 365 -nodes \
            -subj "/CN=localhost" 2>/dev/null
        
        openssl pkcs12 -export \
            -out local.pfx \
            -inkey https/local.key \
            -in https/local.crt \
            -password pass:YourPassword123 2>/dev/null
        cd ..
    fi
    
    echo -e "${GREEN}✓ Проверка завершена${NC}"
    echo ""
}

# Функция для сборки одного сервиса
build_service() {
    local service=$1
    local service_dir="./$service"
    
    echo -e "${BLUE}=== Сборка $service ===${NC}"
    
    # Проверяем существование директории сервиса
    if [ ! -d "$service_dir" ]; then
        echo -e "${RED}Ошибка: Директория $service не найдена${NC}"
        return 1
    fi
    
    # Проверяем наличие dockerfile
    if [ ! -f "$service_dir/dockerfile" ] && [ ! -f "$service_dir/Dockerfile" ]; then
        echo -e "${RED}Ошибка: Dockerfile не найден в $service_dir${NC}"
        return 1
    fi
    
    # Определяем имя Dockerfile
    if [ -f "$service_dir/dockerfile" ]; then
        DOCKERFILE="$service_dir/dockerfile"
    else
        DOCKERFILE="$service_dir/Dockerfile"
    fi
    
    echo -e "${YELLOW}Использую: $DOCKERFILE${NC}"
    echo -e "${YELLOW}Контекст: .. (родительская директория)${NC}"
    
    # Собираем образ
    # Важно! Запускаем из корневой директории, где находятся все сервисы
    # и родительская директория (..) содержит NuGet.config и nuget-local
    cd ..
    
    docker build -f "$service/dockerfile" -t "$service" ..
    
    local build_result=$?
    
    # Возвращаемся в исходную директорию
    cd - > /dev/null
    
    if [ $build_result -eq 0 ]; then
        echo -e "${GREEN}✓ $service успешно собран${NC}"
        return 0
    else
        echo -e "${RED}✗ Ошибка при сборке $service${NC}"
        return 1
    fi
}

# Функция для сборки всех сервисов
build_all() {
    local services=(
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
    
    # Проверяем prerequisites перед сборкой
    check_prerequisites
    
    local failed=0
    local successful=0
    
    for service in "${services[@]}"; do
        if build_service "$service"; then
            successful=$((successful + 1))
        else
            failed=$((failed + 1))
        fi
        echo ""
    done
    
    echo -e "${GREEN}=== Сборка всех сервисов завершена ===${NC}"
    echo -e "${GREEN}✓ Успешно: $successful${NC}"
    if [ $failed -gt 0 ]; then
        echo -e "${RED}✗ С ошибками: $failed${NC}"
    fi
}

# Функция для сборки конкретного сервиса (по аргументу)
build_specific() {
    local service=$1
    
    if [ -z "$service" ]; then
        echo -e "${RED}Ошибка: Укажите имя сервиса${NC}"
        echo "Использование: $0 <имя_сервиса>"
        echo "Доступные сервисы:"
        echo "  planner-auth-service"
        echo "  planner-chat-service"
        echo "  planner-content-service"
        echo "  planner-node-service"
        echo "  planner-notify-service"
        echo "  planner-file-service"
        echo "  planner-email-service"
        echo "  planner-mailbox-service"
        exit 1
    fi
    
    # Проверяем prerequisites перед сборкой
    check_prerequisites
    
    build_service "$service"
}

# Функция для параллельной сборки
build_parallel() {
    local services=(
        "planner-auth-service"
        "planner-chat-service"
        "planner-content-service"
        "planner-node-service"
        "planner-notify-service"
        "planner-file-service"
        "planner-email-service"
        "planner-mailbox-service"
    )
    
    echo -e "${GREEN}=== Параллельная сборка всех сервисов ===${NC}"
    echo ""
    
    # Проверяем prerequisites перед сборкой
    check_prerequisites
    
    # Временная директория для логов
    LOG_DIR="build_logs"
    mkdir -p "$LOG_DIR"
    
    # Запускаем сборку каждого сервиса в фоне
    for service in "${services[@]}"; do
        (
            cd ..
            if docker build -f "$service/dockerfile" -t "$service:latest" . > "../CaesarServer/$LOG_DIR/$service.log" 2>&1; then
                echo -e "${GREEN}[$service] ✓ Успешно${NC}"
            else
                echo -e "${RED}[$service] ✗ Ошибка (лог в $LOG_DIR/$service.log)${NC}"
            fi
        ) &
    done
    
    # Ожидаем завершения всех фоновых процессов
    wait
    
    echo ""
    echo -e "${GREEN}=== Параллельная сборка завершена ===${NC}"
    echo -e "${YELLOW}Логи сохранены в директории $LOG_DIR/${NC}"
}

# Главная функция
main() {
    # Определяем директорию, где находится скрипт
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    
    # Проверяем аргументы командной строки
    case "$1" in
        "all"|"")
            build_all
            ;;
        "parallel")
            build_parallel
            ;;
        "check")
            check_prerequisites
            ;;
        "help")
            echo "Использование: $0 [команда] [сервис]"
            echo ""
            echo "Команды:"
            echo "  all              - собрать все сервисы (по умолчанию)"
            echo "  parallel         - параллельная сборка всех сервисов"
            echo "  check            - только проверить prerequisites"
            echo "  help             - показать эту справку"
            echo ""
            echo "Примеры сборки отдельных сервисов:"
            echo "  $0 planner-auth-service"
            echo "  $0 planner-chat-service"
            echo "  $0 planner-content-service"
            ;;
        *)
            # Если передан не ключ, а имя сервиса - собираем конкретный сервис
            build_specific "$1"
            ;;
    esac
}

# Запускаем главную функцию с аргументами
main "$@"