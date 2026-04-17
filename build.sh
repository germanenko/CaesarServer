#!/bin/bash

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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
    
    # Собираем образ
    # Важно! Запускаем из корневой директории, где находятся все сервисы
    # и родительская директория (..) содержит NuGet.config и nuget-local
    cd "$service"
    
    ./build.sh
    
    local build_result=$?
    
    # Возвращаемся в исходную директорию
    cd ..
    
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
	"planner-analytics-service"
    )
    
    echo -e "${GREEN}=== Начало сборки всех сервисов ===${NC}"
    echo ""

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


# Главная функция
main() {
    # Определяем директорию, где находится скрипт
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    
    # Проверяем аргументы командной строки
    case "$1" in
        "all"|"")
            build_all
            ;;
    esac
}

# Запускаем главную функцию с аргументами
main "$@"