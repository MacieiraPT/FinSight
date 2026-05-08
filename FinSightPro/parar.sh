#!/usr/bin/env bash
echo "A parar FinSight Pro..."
pkill -f "dotnet.*FinSightPro.Web" 2>/dev/null || true
echo "Concluído."
