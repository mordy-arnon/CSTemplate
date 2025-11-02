#!/bin/bash
# Docker Build Script
# Usage: ./scripts/docker-build.sh [tag] [--push] [--no-cache]

set -e

TAG="${1:-latest}"
IMAGE_NAME="cstemplate-api"
REGISTRY="${REGISTRY:-}"
PUSH=false
NO_CACHE=""

# Parse arguments
for arg in "$@"; do
    case $arg in
        --push)
            PUSH=true
            ;;
        --no-cache)
            NO_CACHE="--no-cache"
            ;;
    esac
done

echo "====================================="
echo "  Docker Build"
echo "====================================="
echo ""

# Construct full image name
if [ -n "$REGISTRY" ]; then
    FULL_IMAGE_NAME="$REGISTRY/$IMAGE_NAME:$TAG"
else
    FULL_IMAGE_NAME="$IMAGE_NAME:$TAG"
fi

echo "Image: $FULL_IMAGE_NAME"
echo ""

# Build Docker image
echo "[1/3] Building Docker image..."
docker build \
    -t "$FULL_IMAGE_NAME" \
    -f Dockerfile \
    $NO_CACHE \
    .

# Show image info
echo ""
echo "[2/3] Image built successfully:"
docker images "$FULL_IMAGE_NAME"

# Push if requested
if [ "$PUSH" = true ]; then
    echo ""
    echo "[3/3] Pushing image to registry..."
    docker push "$FULL_IMAGE_NAME"
    
    echo ""
    echo "====================================="
    echo "  ✓ Image built and pushed!"
    echo "  $FULL_IMAGE_NAME"
    echo "====================================="
else
    echo ""
    echo "====================================="
    echo "  ✓ Image built successfully!"
    echo "  $FULL_IMAGE_NAME"
    echo ""
    echo "  To push: ./scripts/docker-build.sh $TAG --push"
    echo "  To run:  docker run -p 5000:5000 $FULL_IMAGE_NAME"
    echo "====================================="
fi

