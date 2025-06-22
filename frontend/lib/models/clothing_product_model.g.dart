// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'clothing_product_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ClothingProductModel _$ClothingProductModelFromJson(
  Map<String, dynamic> json,
) => ClothingProductModel(
  id: json['id'] as String?,
  name: json['name'] as String,
  manufacturer: json['manufacturer'] as String,
  description: json['description'] as String?,
  type: $enumDecode(_$GearTypeEnumMap, json['type']),
);

Map<String, dynamic> _$ClothingProductModelToJson(
  ClothingProductModel instance,
) => <String, dynamic>{
  'id': instance.id,
  'name': instance.name,
  'manufacturer': instance.manufacturer,
  'description': instance.description,
  'type': _$GearTypeEnumMap[instance.type]!,
};

const _$GearTypeEnumMap = {
  GearType.helmet: 0,
  GearType.jacket: 1,
  GearType.trousers: 2,
  GearType.gloves: 3,
  GearType.boots: 4,
  GearType.belt: 5,
  GearType.harness: 6,
  GearType.coverall: 7,
  GearType.vest: 8,
  GearType.other: 9,
};
