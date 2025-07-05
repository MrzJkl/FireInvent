// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'clothing_product_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ClothingProductModel _$ClothingProductModelFromJson(
  Map<String, dynamic> json,
) => ClothingProductModel(
  id: json['id'] as String,
  name: json['name'] as String,
  manufacturer: json['manufacturer'] as String,
  description: json['description'] as String?,
  type: $enumDecode(_$GearTypeEnumMap, json['type']),
);

Map<String, dynamic> _$ClothingProductModelToJson(
  ClothingProductModel instance,
) => <String, dynamic>{
  'name': instance.name,
  'manufacturer': instance.manufacturer,
  'description': instance.description,
  'type': _$GearTypeEnumMap[instance.type]!,
  'id': instance.id,
};

const _$GearTypeEnumMap = {
  GearType.jacket: 0,
  GearType.pants: 1,
  GearType.fireJacket: 2,
  GearType.firePants: 3,
  GearType.helmet: 4,
  GearType.boots: 5,
  GearType.gloves: 6,
  GearType.vest: 7,
  GearType.fireHood: 8,
  GearType.belt: 9,
};
