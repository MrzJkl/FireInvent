// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'clothing_item_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ClothingItemModel _$ClothingItemModelFromJson(Map<String, dynamic> json) =>
    ClothingItemModel(
      id: json['id'] as String?,
      variantId: json['variantId'] as String,
      identifier: json['identifier'] as String?,
      storageLocationId: json['storageLocationId'] as String?,
      condition: $enumDecode(_$GearConditionEnumMap, json['condition']),
      purchaseDate: DateTime.parse(json['purchaseDate'] as String),
      retirementDate:
          json['retirementDate'] == null
              ? null
              : DateTime.parse(json['retirementDate'] as String),
    );

Map<String, dynamic> _$ClothingItemModelToJson(ClothingItemModel instance) =>
    <String, dynamic>{
      'id': instance.id,
      'variantId': instance.variantId,
      'identifier': instance.identifier,
      'storageLocationId': instance.storageLocationId,
      'condition': _$GearConditionEnumMap[instance.condition]!,
      'purchaseDate': instance.purchaseDate.toIso8601String(),
      'retirementDate': instance.retirementDate?.toIso8601String(),
    };

const _$GearConditionEnumMap = {
  GearCondition.excellent: 0,
  GearCondition.good: 1,
  GearCondition.fair: 2,
  GearCondition.poor: 3,
  GearCondition.retired: 4,
};
